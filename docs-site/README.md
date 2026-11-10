# The Z# documentation site

Serves the documentation in `../docs` as a website, with MkDocs and the Material theme.
The pages stay as they are: the navigation, the theme and how code blocks render are
this project's, in `mkdocs.yml`, `hooks.py` and a Pygments lexer for Z#.

From this folder:

```text
uv run mkdocs serve -a 0.0.0.0:8000     serves it, reloading as pages change
uv run mkdocs build                     writes it into site/
```

`0.0.0.0` serves it on every interface, so another device on the network reaches it at
this machine's address, as `http://10.0.0.50:8000`. On Windows, the firewall may ask
whether Python can accept connections the first time.

A page added to `../docs` goes in `nav` in `mkdocs.yml`, to be in the navigation.
