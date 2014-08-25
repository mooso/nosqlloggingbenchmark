# Script written to be dot-sourced to help with building and deploying the Azure service packages.
# Assumes that you have Azure Powershell module installed, and the current subscription is set to a
# a valid subscription.

function Get-ScriptDirectory
{
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value
    Split-Path $Invocation.MyCommand.Path
}

$rootDirectory = Split-Path $(Get-ScriptDirectory)
$solutionDirectory = $rootDirectory

function Ensure-NugetRestored
{
	pushd $solutionDirectory
	nuget restore
	popd
}

function Build-Service($projectName, $flavor = 'Release')
{
	Write-Host "Building package..."
	Ensure-NugetRestored
	pushd "$solutionDirectory\$projectName"
	$buildOutput = msbuild "$projectName.ccproj" /t:Publish "/p:Configuration=$flavor" /p:Platform="AnyCPU" /p:VisualStudioVersion="12.0"
	if ($LASTEXITCODE -ne 0)
	{
		$buildOutput | Write-Host
	}
	popd
}

function Discover-AccountsForLocation($location)
{
	Trap
	{
		return $_
	}
	Get-AzureStorageAccount | ?{$_.GeoPrimaryLocation -eq $location}
}

function Discover-Accounts($serviceName)
{
	Trap
	{
		return $_
	}
	$service = Get-AzureService $serviceName
	Discover-AccountsForLocation $service.Location
}

function Get-ConnectionString([Microsoft.WindowsAzure.Commands.ServiceManagement.Model.StorageServicePropertiesOperationContext]$storageAccount)
{
	$key = Get-AzureStorageKey $storageAccount.StorageAccountName
	"DefaultEndpointsProtocol=https;AccountName=$($storageAccount.StorageAccountName);AccountKey=$($key.Primary)"
}

function Delete-ExistingDeployments([Parameter(Mandatory=$true)]$serviceName)
{
	Write-Host "Deleting existing deployments..."
	try
	{
		$removeOutput = Remove-AzureDeployment -ServiceName $serviceName -Slot Production -Force -DeleteVHD
	} catch {}
}

function Deploy-Service(
	[Parameter(Mandatory=$true)]$projectName,
	[Parameter(Mandatory=$true)]$serviceName,
	[Microsoft.WindowsAzure.Commands.ServiceManagement.Model.StorageServicePropertiesOperationContext]$storageAccount = $null,
	$flavor = 'Release',
	[Switch]$upgradeInPlace)
{
	Trap
	{
		return $_
	}
	$publishDirectory = "$solutionDirectory\$projectName\bin\Release\app.publish"
	if ($storageAccount -eq $null)
	{
		Write-Host "Discovering storage account..."
		$storageAccount = $(Discover-Accounts $serviceName)[0]
	}
	Write-Host "Constructing connection string..."
	$connectionString = Get-ConnectionString $storageAccount
	Write-Host "Writing configuration file..."
	$tempConfigFile = "$env:TEMP\ServiceFinalSettings.cscfg"
	if (Test-Path $tempConfigFile)
	{
		Remove-Item $tempConfigFile -Force
	}
	Get-Content "$publishDirectory\ServiceConfiguration.Cloud.cscfg" |
		%{$_ -replace "UseDevelopmentStorage=true",$connectionString} > $tempConfigFile
	Write-Host "Deploying..."
	if ($upgradeInPlace -and ($existingDeployment = Get-AzureDeployment $serviceName -Sl Production -ErrorAction Ignore))
	{
		$deployment = Set-AzureDeployment -ServiceName $serviceName -Package "$publishDirectory\$projectName.cspkg" -Configuration $tempConfigFile -Label "Deployment on $(Get-Date)" -Slot Production -Upgrade -Force
	}
	else
	{
		Delete-ExistingDeployments $serviceName
        Write-Host "Creating new deployment..."
		$deployment = New-AzureDeployment -ServiceName $serviceName -Package "$publishDirectory\$projectName.cspkg" -Configuration $tempConfigFile -Label "Deployment on $(Get-Date)" -Slot Production
	}
}

function BuildAndDeploy([Parameter(Mandatory=$true)]$projectName,
	[Parameter(Mandatory=$true)]$serviceName,
	[Switch]$upgradeInPlace)
{
	Trap
	{
		return $_
	}
	Build-Service $projectName
	if ($LASTEXITCODE -eq 0)
	{
		Deploy-Service $projectName $serviceName -upgradeInPlace:$upgradeInPlace
	}
}
