"""How the site renders the documentation's pages, which it doesn't change.

The pages' code blocks say what the toolchain's tests do with them: `zs` is a script they
run, `zs fragment` is syntax they don't, and `output` is what the script before it prints.
On the site, a fragment is Z# like any other, and an output is titled as one.

The toolchain's design document, in ../design, is served as the site's architecture pages:
each is added to the site as it is, and the documentation's links to it lead there.
"""

import re
from pathlib import Path

from mkdocs.structure.files import File

FRAGMENT = re.compile(r"^(\s*)```zs fragment\s*$", re.MULTILINE)
OUTPUT = re.compile(r"^(\s*)```output\s*$", re.MULTILINE)
DESIGN = re.compile(r"\]\((?:\.\./)+design/([^)]*)\)")


def on_files(files, config):
    design = Path(config.config_file_path).parent.parent / "design"
    for path in sorted(design.glob("*.md")):
        files.append(File.generated(config, "architecture/" + path.name, abs_src_path=str(path)))
    return files


def on_page_markdown(markdown, page, config, files):
    markdown = FRAGMENT.sub(r"\1```zs", markdown)
    markdown = OUTPUT.sub(r'\1```text title="Output"', markdown)
    if not page.file.src_uri.startswith("architecture/"):
        depth = page.file.src_uri.count("/")
        markdown = DESIGN.sub(lambda match: "](" + "../" * depth + "architecture/" + match.group(1) + ")", markdown)
    return markdown
