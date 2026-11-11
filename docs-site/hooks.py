"""How the site renders the documentation's pages, which it doesn't change.

The pages' code blocks say what the toolchain's tests do with them: `zs` is a script they
run, `zs fragment` is syntax they don't, and `output` is what the script before it prints.
On the site, a fragment is Z# like any other, and an output is titled as one.
"""

import re

FRAGMENT = re.compile(r"^(\s*)```zs fragment\s*$", re.MULTILINE)
OUTPUT = re.compile(r"^(\s*)```output\s*$", re.MULTILINE)


def on_page_markdown(markdown, page, config, files):
    markdown = FRAGMENT.sub(r"\1```zs", markdown)
    markdown = OUTPUT.sub(r'\1```text title="Output"', markdown)
    return markdown
