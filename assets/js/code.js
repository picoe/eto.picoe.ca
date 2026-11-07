// Copy buttons and language tabs for code blocks.
(function () {
  var KEY = 'eto-code-lang';

  function saved() {
    try { return localStorage.getItem(KEY); } catch (e) { return null; }
  }

  function select(lang, remember) {
    document.querySelectorAll('.code-tabs').forEach(function (group) {
      var tabs = group.querySelectorAll('.code-tab');
      // Groups without this language keep what they show.
      if (!group.querySelector('.code-tab[data-lang="' + lang + '"]')) return;
      tabs.forEach(function (tab, i) {
        var on = tab.getAttribute('data-lang') === lang;
        tab.setAttribute('aria-selected', on ? 'true' : 'false');
        tab.tabIndex = on ? 0 : -1;
        group.blocks[i].forEach(function (block) { block.hidden = !on; });
      });
    });
    if (remember) {
      try { localStorage.setItem(KEY, lang); } catch (e) {}
    }
  }

  function buildTabs(group) {
    // One tab per language, holding every block for it, e.g. a .xeto and its code behind.
    var blocks = [];
    var langs = [];
    Array.prototype.forEach.call(group.children, function (c) {
      if (!c.classList.contains('code-block')) return;
      var i = langs.indexOf(c.getAttribute('data-lang'));
      if (i < 0) {
        langs.push(c.getAttribute('data-lang'));
        blocks.push([c]);
      } else {
        blocks[i].push(c);
      }
    });
    if (blocks.length < 2) return;
    group.blocks = blocks;
    var bar = document.createElement('div');
    bar.className = 'code-tabs-bar';
    bar.setAttribute('role', 'tablist');
    blocks.forEach(function (tabBlocks, i) {
      var block = tabBlocks[0];
      var tab = document.createElement('button');
      tab.type = 'button';
      tab.className = 'code-tab';
      tab.setAttribute('role', 'tab');
      tab.setAttribute('data-lang', block.getAttribute('data-lang'));
      tab.textContent = block.getAttribute('data-label');
      tab.setAttribute('aria-selected', i === 0 ? 'true' : 'false');
      tab.tabIndex = i === 0 ? 0 : -1;
      bar.appendChild(tab);
      tabBlocks.forEach(function (b) { b.hidden = i !== 0; });
    });
    // Tabs carry the language name, so the per-block label would repeat it.
    group.classList.add('has-tabs');
    group.insertBefore(bar, group.firstChild);

    bar.addEventListener('click', function (e) {
      var tab = e.target.closest('.code-tab');
      if (!tab) return;
      // Keep the clicked group in place while groups above it change height.
      var top = bar.getBoundingClientRect().top;
      select(tab.getAttribute('data-lang'), true);
      window.scrollBy(0, bar.getBoundingClientRect().top - top);
    });
    bar.addEventListener('keydown', function (e) {
      if (e.key !== 'ArrowRight' && e.key !== 'ArrowLeft') return;
      var tabs = Array.prototype.slice.call(bar.children);
      var i = tabs.indexOf(document.activeElement);
      if (i < 0) return;
      var next = tabs[(i + (e.key === 'ArrowRight' ? 1 : tabs.length - 1)) % tabs.length];
      next.focus();
      next.click();
    });
  }

  function copy(button) {
    var code = button.closest('.code-block').querySelector('pre code');
    if (!code || !navigator.clipboard) return;
    navigator.clipboard.writeText(code.innerText.replace(/\n$/, '')).then(function () {
      button.textContent = 'Copied';
      button.classList.add('copied');
      setTimeout(function () {
        button.textContent = 'Copy';
        button.classList.remove('copied');
      }, 1500);
    });
  }

  document.addEventListener('click', function (e) {
    var b = e.target.closest('.code-copy');
    if (b) copy(b);
  });

  document.querySelectorAll('.code-tabs').forEach(buildTabs);
  var lang = saved();
  if (lang) select(lang, false);
})();
