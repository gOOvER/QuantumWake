/* Translation, for every page the server hands out.
 *
 * English is the key. tx('Last wipe') returns the active catalogue's entry for
 * that text, or the text itself - so an English page needs no catalogue, an
 * untranslated string stays readable, and every test that asserts English
 * still does. Why the rest is shaped as it is: docs/localization.md.
 *
 * tx rather than t: app.js declares a local `t` 52 times, and each of those
 * would quietly shadow a global one inside its function.
 *
 * Loaded after /api/i18n.js, which sets QW_LANG and QW_STRINGS, and before any
 * page script. */

const QW_LANGUAGE = typeof window.QW_LANG === 'string' ? window.QW_LANG : 'en';
const QW_CATALOGUE = window.QW_STRINGS && typeof window.QW_STRINGS === 'object' ? window.QW_STRINGS : {};

/**
 * Which plural form a count takes, per language.
 *
 * Written out rather than asked of Intl.PluralRules: the test engine has no
 * Intl, and the difference that matters fits in a line - French puts 0 with 1
 * ("0 journal"), English puts it with the many ("0 logs").
 */
const QW_PLURAL_RULES = {
  en: (n) => (n === 1 ? 'one' : 'other'),
  fr: (n) => (Math.abs(n) < 2 ? 'one' : 'other'),
};

/** The text as a catalogue key: markup indentation inside a sentence collapsed to single spaces. */
function txKey(text) {
  return String(text ?? '').replace(/\s+/g, ' ').trim();
}

/** Puts named values into {placeholders}; one with no value is left showing rather than blanked. */
function txFill(text, values) {
  if (!values) return text;
  return text.replace(/\{(\w+)\}/g, (whole, name) =>
    (Object.prototype.hasOwnProperty.call(values, name) ? String(values[name]) : whole));
}

function txLookup(english) {
  if (QW_LANGUAGE === 'en') return undefined;
  return QW_CATALOGUE[txKey(english)];
}

/** The text in the page's language, with any {placeholders} filled from values. */
function tx(english, values) {
  const found = txLookup(english);
  return txFill(typeof found === 'string' ? found : english, values);
}

/**
 * A count with its noun, in the page's language: txn(n, '{count} log', '{count} logs').
 *
 * The catalogue keys a plural on the English plural and holds { one, other }.
 * An untranslated plural falls back to English's own rule, not the page's -
 * French's rule applied to English words would print "0 log".
 */
function txn(count, one, other, values) {
  const filled = { count, ...values };
  const found = txLookup(other);

  if (found && typeof found === 'object') {
    const rule = QW_PLURAL_RULES[QW_LANGUAGE] || QW_PLURAL_RULES.en;
    const form = found[rule(count)] ?? found.other;
    if (typeof form === 'string') return txFill(form, filled);
  }

  return txFill(QW_PLURAL_RULES.en(count) === 'one' ? one : other, filled);
}

const TX_ATTRIBUTES = ['title', 'placeholder', 'aria-label', 'alt'];

/* Code is evidence - a file name, a tag, a key - and stays as written. */
const TX_SKIP = new Set(['SCRIPT', 'STYLE', 'CODE', 'PRE', 'TEXTAREA', 'svg']);

/**
 * Translates the markup in place, once, before any page script runs.
 *
 * An element whose whole content is a catalogue key - inline tags and all - is
 * replaced as one, so a sentence with a <code> in it is one sentence for the
 * translator rather than three fragments. Anything else has its text nodes and
 * readable attributes looked up one at a time. It has to happen first: app.js
 * keeps element references from its top level, and an element replaced after
 * that is one the script can no longer reach.
 */
function txDocument(root) {
  if (QW_LANGUAGE === 'en' || !root) return;

  document.documentElement?.setAttribute?.('lang', QW_LANGUAGE);

  const walk = (node) => {
    for (const name of TX_ATTRIBUTES) {
      const value = node.getAttribute?.(name);
      const found = value ? txLookup(value) : undefined;
      if (typeof found === 'string') node.setAttribute(name, found);
    }

    if (TX_SKIP.has(node.tagName)) return;

    const children = Array.from(node.childNodes || []);
    const mixed = node.firstElementChild
      && children.some(child => child.nodeType === 3 && child.nodeValue.trim());

    if (mixed) {
      const found = txLookup(node.innerHTML);
      if (typeof found === 'string') {
        node.innerHTML = found;
        return;
      }
    }

    for (const child of children) {
      if (child.nodeType === 1) {
        walk(child);
        continue;
      }

      if (child.nodeType !== 3) continue;

      const found = txLookup(child.nodeValue);
      if (typeof found !== 'string') continue;

      // Keep the whitespace around the words: it is what separates this text
      // from the inline element beside it.
      const lead = child.nodeValue.match(/^\s*/)[0];
      const trail = child.nodeValue.match(/\s*$/)[0];
      child.nodeValue = lead + found + trail;
    }
  };

  walk(root);
}
