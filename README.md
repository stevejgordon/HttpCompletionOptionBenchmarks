# HttpCompletionOptionBenchmarks

This repo contains some basic, quick and dirty benchmarks for HttpClient to compare the benefit of HttpCompletionOption.ResponseHeadersRead vs. the default of HttpCompletionOption.ResponseContentRead.

NOTE: This isn't extensively well designed but provides enough data for a rough comparison.

## Getting Started

The project contains two projects. A basic API to call and the benchmarks. In theory I could have avoided the need for the API project but it was the quickest option at the time.

To run the benchmarks you will need to set multiple startup projects so that the API is running as well as the benchmark project.