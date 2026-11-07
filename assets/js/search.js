// Header search: API names from Owl's index first, then full text over the guides and API from the
// Pagefind index build.sh writes.
(function () {
  var box = document.querySelector('.site-search');
  if (!box) return;
  var input = box.querySelector('input');
  var list = box.querySelector('.site-search-results');
  var NAME_LIMIT = 5;
  var TEXT_LIMIT = 6;
  var loading = null;
  var rows = [];
  var current = -1;

  function load() {
    // Loaded on first use so pages that are never searched don't fetch either index.
    if (!loading) {
      var pagefind = import(box.getAttribute('data-pagefind-url'))
        .then(function (pf) {
          pf.options({ excerptLength: 16 });
          pf.init();
          return pf;
        })
        .catch(function () {
          return null;
        });
      var names = fetch(box.getAttribute('data-api-index-url'))
        .then(function (r) {
          return r.ok ? r.json() : null;
        })
        .then(function (index) {
          // Owl's own format; a version this doesn't know just leaves the name matches out.
          return index && index.schema === 6 ? index : null;
        })
        .catch(function () {
          return null;
        });
      loading = Promise.all([pagefind, names]);
    }
    return loading;
  }

  function escape(s) {
    return String(s).replace(/[&<>"]/g, function (c) {
      return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c];
    });
  }

  // Owl's dropdown order (findMatches in owl-site/lib/assets.js): interfaces, then other types, then
  // members, each by how closely the name matches, and only rows whose name holds the query.
  function nameMatches(index, q) {
    q = q.toLowerCase();
    var qualified = q.indexOf('.') >= 0;
    var matches = [];
    for (var i = 0; i < index.entries.length; i++) {
      var e = index.entries[i];
      var name = e[0].toLowerCase();
      if (name.charAt(name.lastIndexOf('.') + 1) === '~') continue;
      var ns = qualified ? index.namespaces[e[6]] : '';
      var full = ns && ns !== '(global)' ? ns.toLowerCase() + '.' + name : name;
      var subject = qualified ? name : name.slice(name.lastIndexOf('.') + 1);
      var rank = subject === q ? 0 : subject.indexOf(q) === 0 ? 1 : subject.indexOf(q) >= 0 ? 2 : -1;
      if (rank < 0 && qualified) rank = full === q ? 0 : full.indexOf(q) === 0 ? 1 : full.indexOf(q) >= 0 ? 2 : -1;
      if (rank < 0) continue;
      var group = !(e[9] & 2) ? 2 : e[8] === 'interface' ? 0 : 1;
      matches.push([group, rank, e]);
    }
    matches.sort(function (a, b) {
      return a[0] - b[0] || a[1] - b[1] || a[2][0].length - b[2][0].length || a[2][0].localeCompare(b[2][0]);
    });
    return matches.slice(0, NAME_LIMIT).map(function (m) {
      return { url: m[2][1], title: m[2][0], tag: m[2][8] || 'API', text: escape(m[2][3] || '') };
    });
  }

  function textMatches(pf, q, skip) {
    return pf.debouncedSearch(q, {}, 120).then(function (search) {
      // Null when a newer keystroke replaced this search.
      if (!search) return null;
      var hits = search.results.slice(0, TEXT_LIMIT + NAME_LIMIT);
      // Eto's platform plumbing matches almost any control's name, so it's only offered when asked for.
      var plumbing = /\.(IHandler|ICallback|Callback)$/;
      var wantsPlumbing = /handler|callback/i.test(q);
      return Promise.all(hits.map(function (r) { return r.data(); })).then(function (data) {
        var pages = data
          .filter(function (r) { return !skip[r.url] && (wantsPlumbing || !plumbing.test(r.meta.title || '')); })
          .map(function (r) {
            // Pagefind's excerpt is page text with its matches already wrapped in <mark>.
            return { url: r.url, title: r.meta.title || r.url, tag: r.url.indexOf('/api/') === 0 ? 'API' : 'Docs', text: r.excerpt };
          });
        // Guides first: an API page is usually reached by name, above.
        return pages
          .filter(function (p) { return p.tag === 'Docs'; })
          .concat(pages.filter(function (p) { return p.tag !== 'Docs'; }))
          .slice(0, TEXT_LIMIT);
      });
    });
  }

  function row(r) {
    return (
      '<a class="site-search-row" role="option" href="' + escape(r.url) + '">' +
      '<span class="site-search-title"><span>' + escape(r.title) + '</span>' +
      '<span class="site-search-tag">' + escape(r.tag) + '</span></span>' +
      (r.text ? '<span class="site-search-excerpt">' + r.text + '</span>' : '') + '</a>'
    );
  }

  function section(label, items) {
    return items.length ? '<div class="site-search-heading">' + label + '</div>' + items.map(row).join('') : '';
  }

  function show(html) {
    list.innerHTML = html;
    open(!!html);
    rows = Array.prototype.slice.call(list.querySelectorAll('a.site-search-row'));
    select(-1);
  }

  function open(on) {
    list.hidden = !on;
    input.setAttribute('aria-expanded', on ? 'true' : 'false');
  }

  function select(i) {
    if (rows[current]) rows[current].removeAttribute('aria-selected');
    current = i;
    if (rows[current]) {
      rows[current].setAttribute('aria-selected', 'true');
      rows[current].scrollIntoView({ block: 'nearest' });
    }
  }

  function run() {
    var q = input.value.trim();
    if (!q) {
      show('');
      return;
    }
    load().then(function (indexes) {
      var pf = indexes[0];
      var names = indexes[1] ? nameMatches(indexes[1], q) : [];
      if (!pf && !indexes[1]) {
        show('<div class="site-search-note">Search isn’t available here. It is built by build.sh.</div>');
        return;
      }
      // Pages already offered by name aren't offered again under their text.
      var skip = {};
      names.forEach(function (n) { skip[n.url.split('#')[0]] = true; });
      var text = pf ? textMatches(pf, q, skip) : Promise.resolve([]);
      return text.then(function (pages) {
        if (pages === null || input.value.trim() !== q) return;
        var html = section('API', names) + section('Pages', pages);
        show(html || '<div class="site-search-note">No results for “' + escape(q) + '”</div>');
      });
    });
  }

  input.addEventListener('focus', load);
  input.addEventListener('input', run);

  input.addEventListener('keydown', function (e) {
    if (e.key === 'ArrowDown' && rows.length) {
      e.preventDefault();
      select((current + 1) % rows.length);
    } else if (e.key === 'ArrowUp' && rows.length) {
      e.preventDefault();
      select(current <= 0 ? rows.length - 1 : current - 1);
    } else if (e.key === 'Enter') {
      var target = rows[current] || rows[0];
      if (target) {
        e.preventDefault();
        location.href = target.href;
      }
    } else if (e.key === 'Escape') {
      input.value = '';
      show('');
      input.blur();
    }
  });

  // Closes when a click or focus leaves the box, but not while a result is being clicked.
  document.addEventListener('mousedown', function (e) {
    if (!box.contains(e.target)) open(false);
  });
  input.addEventListener('blur', function () {
    setTimeout(function () {
      if (!box.contains(document.activeElement)) open(false);
    }, 0);
  });
  input.addEventListener('focus', function () {
    if (list.innerHTML && input.value.trim()) open(true);
  });

  // "/" jumps to the box from anywhere that isn't already taking text.
  document.addEventListener('keydown', function (e) {
    if (e.key !== '/' || e.metaKey || e.ctrlKey || e.altKey) return;
    var t = e.target;
    if (t && (t.isContentEditable || /^(INPUT|TEXTAREA|SELECT)$/.test(t.tagName))) return;
    e.preventDefault();
    input.focus();
    input.select();
  });
})();
