# UmbHttpStatusCode
Event handler plugin for Umbraco that allows users to specify as page properties HTTP Status Codes to be returned.

## What's inside
This project includes a DLL that will register as an event handler with Umbraco. If pages are specified with the proper properties, it will set those as HTTP status and substatus codes for the pages.

## System requirements
1. NET Framework 4.5
2. Umbraco 7.3.7+ (should work with older versions but not tested)

# NuGet availability
This project is available on [NuGet](https://www.nuget.org/packages/UmbHttpStatusCode/).

## Usage instructions
### Getting started
1. Add **UmbHttpStatusCode.dll** as a reference in your project or place it in the **\bin** folder.
2. Add two numeric-type page properties in Umbraco:
  - `umbHttpStatusCode`
  - `umbHttpSubStatusCode`
