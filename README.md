# Blackbird.io Widn

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

**Widn** is translation AI for everyone. Widn’s AI provides accurate translations, preserving meaning, maintaining brand voice, and cultural nuances across +20 language pairs and various domains.

## Before Setting Up

- After signing in at [https://www.widn.ai/](https://www.widn.ai/), go to the toolbar on the left and click on **API Key.**
- Generate your API key.

## Connecting

1. Navigate to **Apps** and identify the Widn app. You can use the search to find it.
2. Click *Add Connection*.
3. Name your connection for future reference, e.g., "My Widn connection."
4. Fill in the API key from your Widn account.
5. Click *Connect*.

## Actions

### Translation

- **Translate text** translates plain text.
- **Translate file** translates a file. Widn supports many different file types. See Widn documentation.

Widn offers translation inputs for model selection, extra instructions, and a glossary.

### Glossary

- **Export glossary** exports a glossary file to use with other applications.
- **Import glossary** imports a glossary that was exported from a different app.

### Quality

- **Evaluate translation quality** evaluates the quality of a translation given a reference text using `xcomet-xl`.
- **Estimate translation quality** estimates the quality of a translation, without a reference text. You can choose between `xcomet-xl` and `mqm-qe`.
- **Estimate XLIFF translation quality** estimates the quality of an entire translated XLIFF file with the same options as the previous action.


![Frame 1](image/README/Instructions.png)

You can now translate files by uploading your file with the source and target languages. XLIFF translation is already available via the Widn API, alongside PDF and these other file types: [Supported File Formats](https://help.unbabel.com/hc/en-us/articles/11241362647447-Supported-file-formats).

## Examples

### Slack Bird

![Slack Example](image/README/SlackBird.png)

The example above shows Widn AI being used as a Slack integration.

### Email Bird

![Email Example](image/README/EmailBird.png)

The image above shows a workflow that, whenever a new email comes in, the email content is sent for translation using [widn.ai](http://widn.ai/). Afterward, it is sent to a Slack channel with the email translated into the desired language. This is another way you can utilize [widn.ai](http://widn.ai/).

## Feedback

Do you want to use this app or have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->