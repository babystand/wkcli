# Wani Kani CLI

## WaniKani reviews from the comfort of the terminal!

Pros:

- Provides kana input for Japanese reading review items without the need for an IME
- Displays additional information and mnemonics to submitted answers
- More condensed/streamlined than the web experience
- Makes it look like you're hard at work doing hacker things
- Shortcuts for converting number input into words (21 -> "twenty one")

Cons:

- Some WaniKani radicals don't have Unicode points, best-effort prompts are as good as it gets for now
- Does not (yet) provide a view for the results page at the end of a review
- Does not (yet) work for lessons
- If you somehow get navigated outside of the login, dashboard, or review pages the program will HACF


#### Tech
- F#
- .NET Core 2.1
- [Canopy](https://github.com/lefthandedgoat/canopy)
- [Colorful.Console](https://github.com/tomakita/Colorful.Console)
