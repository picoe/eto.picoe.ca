// Home page features: highlights the step in the middle of the screen and shows its image beside it.
(function () {
  var root = document.querySelector('.features');
  if (!root || !('IntersectionObserver' in window)) return;
  var steps = root.querySelectorAll('.feature-step');
  var images = root.querySelectorAll('.features-stage img');
  root.classList.add('is-scrolly');

  function activate(index) {
    steps.forEach(function (s, i) { s.classList.toggle('is-active', i === index); });
    images.forEach(function (img, i) { img.classList.toggle('is-active', i === index); });
  }

  // A thin band across the middle of the viewport, so only one step counts as "current".
  var observer = new IntersectionObserver(function (entries) {
    entries.forEach(function (e) {
      if (e.isIntersecting) activate(+e.target.getAttribute('data-step'));
    });
  }, { rootMargin: '-45% 0px -45% 0px' });
  steps.forEach(function (s) { observer.observe(s); });
})();
