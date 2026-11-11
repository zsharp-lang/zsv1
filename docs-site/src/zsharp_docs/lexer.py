"""A Pygments lexer for Z#, so the site highlights its code blocks."""

from pygments.lexer import RegexLexer, bygroups, words
from pygments.token import Comment, Keyword, Name, Number, Operator, Punctuation, String, Text, Whitespace

KEYWORDS = (
    "as", "break", "case", "continue", "def", "else", "for", "from", "fun", "get", "if",
    "import", "in", "is", "let", "module", "new", "of", "or", "return", "set", "var",
    "when", "while",
)

CONSTANTS = ("true", "false", "null", "this")


class ZSharpLexer(RegexLexer):
    name = "Z#"
    aliases = ["zs", "zsharp"]
    filenames = ["*.zs"]

    tokens = {
        "root": [
            (r"\s+", Whitespace),
            (r"//.*?$", Comment.Single),
            (r'"(\\\\|\\"|[^"])*"', String),
            (r"@[A-Za-z_]\w*", Name.Decorator),
            (r"(fun|def|module|new)(\s+)([A-Za-z_]\w*)", bygroups(Keyword, Whitespace, Name.Function)),
            (words(KEYWORDS, suffix=r"\b"), Keyword),
            (words(CONSTANTS, suffix=r"\b"), Keyword.Constant),
            (r"[A-Z]\w*", Name.Class),
            (r"[a-z_]\w*", Name),
            (r"\d+\.\d+", Number.Float),
            (r"\d+", Number.Integer),
            (r"==|!=|<=|>=|\*\*|[-+*/=<>&|]", Operator),
            (r"[{}()\[\];,.:]", Punctuation),
            (r".", Text),
        ],
    }
