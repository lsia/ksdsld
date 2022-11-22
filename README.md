# ksdsld - Keystroke Dynamics Synthesis &amp; Liveness Detection

### Introduction

This software accompanies the article [1] and is provided as part of the article _KSDSLD - A Tool for Keystroke Dynamics Synthesis \& Liveness Detection_, to be published in Software Impacts, Elsevier. 

With the objective of encouraging research about liveness detection in keystroke dynamics and making available our methods so other verification systems can be evaluated against them, we provide software and its source code that includes the following functionality:

* Forges synthesized samples of free-text keystroke dynamics, given their target texts and one or more authentic samples by the legitimate user to be impersonated.
* Trains classification models to distinguish human-written samples from synthetic forgeries, given a collection of samples from the legitimate user.
* Performs liveness detection with the above models, given a collection of samples of unknown origin to be verified and flagged as human-written or synthesized. 

### Requirements

The tool is provided as a pre-compiled command line utility together with its source code, but the executable can be imported as a library assembly if required. The source code was written in C# and requires Visual Studio 2022 to be compiled; free Community versions should be sufficient for the task. .NET Framework 4.8 SDK is needed at compile time and .NET Framework 4.8 at runtime. WEKA 3.8.2 is required at run time for the liveness detection functionality.

No Windows- or Microsoft-specific features in the execution environment are required by the software, and in principle it should be possible to compile and make it run using alternative implementations of the .NET Framework, like Mono. However, no explicit effort has been made to enforce extended compatibility or portability. 


### Sample files format

Input and output takes the form of CSV files. Each input sample is represented by CSV file with a fixed three column structure including virtual key code, hold time, and flight time for each keystroke, as shown below. Invalid or unknown timing values are represented with negative numbers. Samples by each user are grouped by folders. 

1. **VK**. _Virtual key code__. Virtual key code identifying the current keystroke, as defined by [Microsoft reference](https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes)
2. **HT**. __Hold time__. Interval between key press and key release event, in milliseconds.
3. **FT**. __Flight time__. Flight time & Interval between the previous key press event and this key press event, in milliseconds. Note that the first keystroke in the sample cannot have a valid flight time, because there is no previous key press event.



### Sample synthesis


Sample synthesis requires as input the name of the synthesis method to employ, a set of human-written samples by the legitimate user, and a set of samples to provide the target text which keystroke dynamics will be synthesized. The output is a set of synthesized samples, in the aforementioned CSV format, sharing the target texts of the latter. For details about the synthesis methods, please refer to \[1\].

The command line syntax for sample synthesis is

KSDSLD {legitimateUserSamplesFolder} {targetTextSamplesFolder} SYNTHESIZE {method}

where the required parameters are

1. **legitimateUserSamplesFolder**. Folder where the human-written samples of the legitimate user are located. The samples must abide to the CSV format described in the previous section. All samples in the folder are used as input to the behavioral model for sample synthesis.
2. **targetTextSamplesFolder**. Folder where the samples with target text are located. The samples must abide to the CSV format described in the previous section, but their timing information is ignored and only the virtual key sequence is considered. The synthesized output samples are also stored here.
3. **method**. Method used to synthesize the output samples.

The supported synthesis methods are

1. AVERAGE
2. UNIFORM
3. GAUSSIAN
4. HISTOGRAM
5. NSHISTOGRAM

The file names of the output samples will have the form _ORIGINAL_-SYNTHESIZED-_METHOD_.csv_, where the fields denote

1. **ORIGINAL**. File name of the original sample with target text, excluding extension.
2. **METHOD**. Method used to synthesize the output sample.



### Liveness detection

Liveness detection requires as input a set of human-written samples by the legitimate user and a set of samples to be verified. The output is a single CSV file enumerating the verdict for each of the latter, whether it is classified as human-written (legitimate) or synthesized (impostor). 

The command line syntax for sample synthesis is

KSDSLD {legitimateUserSamplesFolder} {toBeVerifiedSamplesFolder} VERIFY 

where the required parameters are

1. **legitimateUserSamplesFolder**. Folder where the human-written samples of the legitimate user are located. The samples must abide to the CSV format described in the previous section. All samples in the folder are used as input to the behavioral model for liveness detection.
2. **toBeVerifiedSamplesFolder**. Folder where the samples to be verified are located. The samples must abide to the CSV format described in the previous section. 

The output file will be named _RESULTS.csv_ and will be created on the current working folder. It will have two columns, and one row for each sample in the _toBeVerifiedSamplesFolder_. The columns are

1. **sample**. File name of the sample that was verified.
2. **verdict**. Verdict of the classification. _legitimate_ for samples detected as human-written and _impostor_ for samples detected as synthesized.


### References

[1] Nahuel González, Enrique P. Calot, Jorge S. Ierache, and Waldo Hasperué. Towards liveness detection in keystroke dynamics: Revealing synthetic forgeries. Systems and Soft Computing, 4:200037, 2022. ISSN 2772-9419. doi: [https://doi.org/10.1016/j.sasc.2022.200037](https://doi.org/10.1016/j.sasc.2022.200037). URL [https://www.sciencedirect.com/science/article/pii/S2772941922000047](https://www.sciencedirect.com/science/article/pii/S2772941922000047).