# BooruTagger

A very simple WPF app to show image thumbnails and 'tags'. I use it to assist in 'tagging' images downloaded from boorus.

This is _not_ fancy: no MVVM, MVC or any of that jazz. Kludgy dialogs, etc. _Not_ a "good" example of how to program WPF. Heck, not a good example of how to develop a GUI!

These facts may change over time.

Program
-------
Added a pre-build executable in its own folder. You'll need .NET 4.5 installed to run it.

Tags
----

When I download images from boorus (Danbooru, Gelbooru, etc) I 'tag' the images via the filename, separated by '+' (plus). E.g.:

```
Danbooru_1901349+1girl+animal_ears+blush+brown_hair+fangs+highres+holo+hooded_cloak+kawakami_rokkaku+long_hair+looking_at_viewer+mittens+open_mouth+red_eyes+smile+snow+snowball+solo+spice_and_wolf+tail
```

is an image downloaded from Danbooru.

Tags are either as come from a booru or as added by myself. 

Some obvious limitations:
- Windows has a 256 character limit, including the full path to the image. Not many tags.
- There are a lot of tags from the booru, many of which I don't care about.
- Spelling errors, inconsistent tags, etc.

So this program is intended to help deal with these limits/issues:

1. Select a folder: view thumbnails and all tags for all images in the folder.
2. Be able to delete all, or some, instances of a tag.
3. Be able to edit a tag and have that apply to all images.
4. Be able to add a new/existing tag to one or more images.
5. Be smart enough to manage the 256 character limit, TBD: possibly by dropping "low priority" tags.
