# Kontent.Wyam

![CI](https://github.com/alanta/Kontent.Wyam/workflows/CI/badge.svg?branch=master)

Module for retrieving [Kentico Kontent](https://kontent.ai) content items, for generating static websites with [Wyam](https://wyam.io).

> ⚠️ [Wyam](https://github.com/Wyamio/Wyam) is succeeded by [Statiq](https://github.com/statiqdev/Statiq.Framework) <br/>
> If you're starting a new static site project with Kontent, please use [Kontent.Statiq](https://github.com/alanta/Kontent.Statiq).
> There will be no active development on this repository, PRs are still welcome though.

## Getting started

* Setup a project on [Kontent](https://app.kontent.ai/sign-up). You can use the [demo project](https://docs.kontent.ai/tutorials/set-up-kontent/projects/manage-projects#a-creating-a-sample-project) to get going.
* Install 
* Setup Wyam pipeline in `config.wyam`, for example:

```c#
#n Konent.Wyam
#n Kentico.Kontent.Delivery

Pipelines
    .Add("Kontent", 
    Kontent("0000000-0000-0000-0000-000000000000") // enter your API key
        .WithContentType("article")
        .WithContentField("body_copy")
    KontentAssetParser(),
    WriteFiles($"{@doc["url_pattern"]}.html")
);
```

* Run the wyam pipeline from a command prompt
* You should find an HTML file in the `output` folder for each article defined in the Kontent project.

This gives you everything you need to get started with content served from a Kontent project. You need to specify the type of content to fetch by calling `.WithContentType("...")` and what property contains the main content by calling `.WithContentField("...")`. All properties on the content item are copied into the [Wyam Document](https://wyam.io/docs/concepts/documents) [Metadata](https://wyam.io/docs/concepts/metadata).

But Kontent has more advanced features and some nice extra's that you can leverage to get better integration and more robust code.

## Typed content

The Kontent Delivery Client for .NET can map the content onto plain C# objects. There's also a generator tool, [Kontent generator](https://github.com/Kentico/kontent-generators-net), that can generate a C# class for each of the content types in your project as well as a TypeProvider that maps type names to C# classes and vice versa.

This makes it really easy to work with the content in C# and, for example, Razor views. The generator is also helpful in keeping your web application code in sync with the project's content model. Giving compiler errors when properties change so you know what to fix.

To generate these classes run the tool and specify the options you prefer:

```cmd
KontentModelGenerator --projectid "00000000-0000-0000-0000-000000000000" --outputdir Models --namespace My.Models -s true -g true
```

In order to use these classes in the Wyam pipeline, you need to compile this into an assembly first.

The easiest way to do that is to use 

## Inline content

Kontent allows you to have content within content. Which is very powerfull but requires a bit of work on the client side to make it work.
You basically have two options:

* _Inline resolvers_
  These are called by the Kontent Delivery Client to transform inline content items into HTML. They're nice for simple models with very basic HTML.
  Inline resolvers enable the Delivery API client to map structured content directly to HTML. This is achieved by making the property on the typed content class a string.
* _Structured content_
  You can also use the structured content in your application. This is achieved by making the content property of type `IRichTextContent`. This allows you to render the inline content in views or what ever code is appropriate.

Both these models can be used with Wyam, it's up to your preferences.

Where ever Wyam hands you a `Document`, use the extension method `.AsKontent<TModel>()` to get the typed model from the document.

## Setup for structured content

First, you need to let the Delivery API know how to resolve all the types of content you have to C# classes. The Kontent API uses an instance of ITypeProvider for that and it's generated for you if you use the Kontent Generator tool:

```c#
#n Konent.Wyam
#n Kentico.Kontent.Delivery
// your compiled content models :
#a My.Models.dll 

Kontent("0000000-0000-0000-0000-000000000000") // enter your API key
   .WithTypeProvider<My.Models.CustomTypeProvider>() // your type provider
```

## Setup Inline resolvers (optional)

_TODO_

## Trouble shooting

> There are weird object tags like this in my content: 

```xml
<object type="application/kenticocloud" data-type="item" data-rel="component" data-codename="n2ef9e997_4691_0118_8777_c0ac9cee683b"></object>
```

Make sure you read the section on structured content and follow the configuration steps.

## How do I build this repo?

You'll need a .NET Core development setup: Windows, Mac, Linux with VisualStudio, VS Code or Rider.

## Contribution guidelines

* You're welcome to send pull requests. Please create an issue first and include a unit test with your pull request to verify your code.
* Massive refactorings, code cleanups etc. will be rejected unless explictly agreed upon
* Adding additional Nuget package dependencies to the main assemblies is strongly discouraged

## Who do I talk to?

* Marnix van Valen on twitter : @marnixvanvalen or e-mail : marnix [at] alanta [dot] nl

## Blog posts & docs

[Static sites with Kentico Cloud, Wyam and Netlify](https://www.kenticotricks.com/blog/static-sites-with-kentico-cloud) Kristian Bortnik, 31 jan 2018
