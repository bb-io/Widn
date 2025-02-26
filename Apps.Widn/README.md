# Blackbird.io Widn

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

Widn is a translation AI providing accurate translations, preserving meaning and nuance across +20 language pairs and various domains.

## Before setting up

Before you can connect you need to make sure that:

- You have access to a Widn API key.

## Connecting

1. Navigate to Apps, and identify the Widn app. You can use search to find it.
2. Click _Add Connection_.
3. Name your connection for future reference e.g. 'My Widn connection'.
4. Fill in the API key to your Widn account.
5. Click _Connect_.

## Actions

### Quality Estimation

- **Evaluate translation quality** evaluates the quality of a translation. Requires a source, target language and  reference text.
- **Estimate XLIFF translation quality** estimates the quality of a translation from an XLIFF file
- **Estimate translation quality**  estimate the quality of a translation

### Translation 

- **Translate text** translates plain text.
- **Translate file** translates the input file. Supported formats: csv, dita, ditamap, docm, docx, dtd, htm, html, icml, idml, json, markdown, md, mif, mqxliff, mxliff, odp, ods, odt, ots, po, potm, potx, ppsm, ppsx, pptm, pptx, properties, resx, sdlxliff, strings, stringsdict, tmx, tsv, vsdx, xml, yaml, yml.

Widen offers translation inputs for model selection, tone, extra instructions and a glossary.

### Glossaries 

- **Import glossary** imports glossary (.tbx, .csv & .tsv).
- **Export glossary** exports glossary

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
