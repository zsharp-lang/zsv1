"""How the site renders the documentation's pages, which it doesn't change.

The pages' code blocks say what the toolchain's tests do with them: `zs` is a script they
run, `zs fragment` is syntax they don't, and `output` is what the script before it prints.
On the site, a fragment is Z# like any other, and an output is titled as one.

Pages that live with what they describe are served as the site's too, each as it is:
the design document, in ../design, as the architecture pages, and each standard library's
README, in ../std, as its page under the libraries. The documentation's links to them lead
to those pages.
"""

import re
from pathlib import Path

from mkdocs.structure.files import File

FRAGMENT = re.compile(r"^(\s*)```zs fragment\s*$", re.MULTILINE)
OUTPUT = re.compile(r"^(\s*)```output\s*$", re.MULTILINE)
DESIGN = re.compile(r"\]\((?:\.\./)+design/([^)]*)\)")
LIBRARY = re.compile(r"\]\((?:\.\./)+std/([^)]*)/README\.md\)")


def served(root):
    """Each page served from outside the documentation, by where the site has it."""
    for path in sorted((root / "design").glob("*.md")):
        yield "architecture/" + path.name, path
    for path in sorted((root / "std").rglob("README.md")):
        if "build" not in path.parts:
            yield "library/std/" + path.parent.relative_to(root / "std").as_posix() + ".md", path


def on_files(files, config):
    root = Path(config.config_file_path).parent.parent
    for uri, path in served(root):
        files.append(File.generated(config, uri, abs_src_path=str(path)))
    return files


def on_page_markdown(markdown, page, config, files):
    markdown = FRAGMENT.sub(r"\1```zs", markdown)
    markdown = OUTPUT.sub(r'\1```text title="Output"', markdown)
    up = "../" * page.file.src_uri.count("/")
    if not page.file.src_uri.startswith(("architecture/", "library/std/")):
        markdown = DESIGN.sub(lambda match: "](" + up + "architecture/" + match.group(1) + ")", markdown)
        markdown = LIBRARY.sub(lambda match: "](" + up + "library/std/" + match.group(1) + ".md)", markdown)
    return markdown
