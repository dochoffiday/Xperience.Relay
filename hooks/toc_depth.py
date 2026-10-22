"""Lets a page limit how deep its table of contents goes.

A page that says `toc_depth: 2` in its front matter lists only its level 2 headings in the table of contents (the right-hand
column of the page, and the entry under the page in the navigation on a small screen). The pages of the command areas use it:
every command is a level 2 section, and the sections inside a command (Parameters, Result, Examples, ...) are repeated for
each of them, so listing those too would bury the commands.

Python-Markdown's `toc_depth` can only be set for the whole site, which would also hide the level 3 headings of pages that
want them, such as the configuration page.
"""


def on_page_context(context, page, config, nav):
    depth = page.meta.get("toc_depth")

    if depth:
        _prune(page.toc.items, int(depth))

    return context


def _prune(items, depth):
    for item in items:
        if item.level >= depth:
            item.children = []
        else:
            _prune(item.children, depth)
