using GeneralUpdate.Core;

/*
 * GeneralUpdateOSS will by default read the JSON content of GlobalConfigInfoOSS stored in the system environment variables by GeneralClientOSS
 * , and developers do not need to be concerned with the entire process.
 * 
 * Environment.GetEnvironmentVariable("GlobalConfigInfoOSS", EnvironmentVariableTarget.User);
 * 
 * Typically, GeneralClientOSS and GeneralUpdateOSS appear as a pair.
 */
GeneralUpdateOSS.Start();