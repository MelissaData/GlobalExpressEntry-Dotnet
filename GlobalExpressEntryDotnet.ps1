<#
.SYNOPSIS
    Builds and runs the Melissa Global Express Entry Cloud API .NET sample.

.DESCRIPTION
    This script builds GlobalExpressEntryDotnet with dotnet publish, then runs the
    resulting executable, passing along the license and (if supplied) the address fields.

    Overall flow:
      1. Resolve the license (parameter, prompt, or MD_LICENSE environment variable).
      2. Publish GlobalExpressEntryDotnet in Release configuration to
         .\GlobalExpressEntryDotnet\Build.
      3. Run the built executable: one-shot mode if any address field was supplied,
         otherwise interactive mode (the .NET program prompts for each field).

.PARAMETER addressline1
    Address line 1 to look up in one-shot mode.

.PARAMETER city
    City to look up in one-shot mode.

.PARAMETER state
    State to look up in one-shot mode.

.PARAMETER postal
    Postal code to look up in one-shot mode.

.PARAMETER license
    License string. Resolved in this order:
      1. This parameter.
      2. An interactive prompt, if the parameter was not supplied.
      3. The MD_LICENSE environment variable, if the prompt was left blank.
    Note that the environment variable is the last resort, not the first: running
    without -license always prompts, even when MD_LICENSE is set.

.PARAMETER quiet
    Accepted for parity with other sample scripts; not currently used to suppress output.

.EXAMPLE
    .\GlobalExpressEntryDotnet.ps1 -license "your-license"

.EXAMPLE
    .\GlobalExpressEntryDotnet.ps1 -addressline1 "22382 Avenida Empresa" -city "Rancho Santa Margarita" -state "CA" -postal "92688" -license "your-license"
#>

######################### Parameters ##########################
param(
    $addressline1 = '',
    $city = '',
    $state = '',
    $postal = '',
    $license = '',
    [switch]$quiet = $false
    )

# Uses the location of the .ps1 file
$CurrentPath = $PSScriptRoot
Set-Location $CurrentPath
$ProjectPath = "$CurrentPath\GlobalExpressEntryDotnet"
$BuildPath = "$ProjectPath\Build"

If (!(Test-Path $BuildPath)) {
  New-Item -Path $ProjectPath -Name 'Build' -ItemType "directory"
}

########################## Main ############################
Write-Host "`n==================== Melissa Global Express Entry Cloud API =====================`n"

# Get license (either from parameters or user input)
if ([string]::IsNullOrEmpty($license) ) {
  $license = Read-Host "Please enter your license string"
}

# Check for License from Environment Variables 
if ([string]::IsNullOrEmpty($license) ) {
  $license = $env:MD_LICENSE 
}

if ([string]::IsNullOrEmpty($license)) {
  Write-Host "`nLicense String is invalid!"
  Exit
}

# Start program
# Build project
Write-Host "`n=================================== BUILD PROJECT ================================="

dotnet publish -f="net7.0" -c Release -o $BuildPath GlobalExpressEntryDotnet\GlobalExpressEntryDotnet.csproj

# Run project
# No address fields supplied -> run interactively; otherwise pass the supplied ones through for one-shot mode.
if ([string]::IsNullOrEmpty($addressline1) -and [string]::IsNullOrEmpty($city) -and [string]::IsNullOrEmpty($state) -and [string]::IsNullOrEmpty($postal)) {
  dotnet $BuildPath\GlobalExpressEntryDotnet.dll --license $license
}
else {
  # Only pass flags that have a value. Windows PowerShell drops empty-string arguments to
  # native programs, which would shift the next flag name into this flag's value.
  # Any field left out here is prompted for by the program.
  $runArgs = @('--license', $license)
  if (-not [string]::IsNullOrEmpty($addressline1)) { $runArgs += '--addressline1', $addressline1 }
  if (-not [string]::IsNullOrEmpty($city))         { $runArgs += '--city', $city }
  if (-not [string]::IsNullOrEmpty($state))        { $runArgs += '--state', $state }
  if (-not [string]::IsNullOrEmpty($postal))       { $runArgs += '--postal', $postal }
  dotnet $BuildPath\GlobalExpressEntryDotnet.dll @runArgs
}
