param(
    [string]$path
)
$ErrorActionPreference = "Stop"

if ([System.IO.File]::Exists($path) -eq $false) {
    throw """$path"" does not exist"
}

# Helper function to encrypt stuff, found it on the internet.
$load = [Reflection.Assembly]::LoadWithPartialName("System.Security")
function Encrypt-String($String, $Passphrase, $salt="My Voice is my P455W0RD!", $init="Yet another key", [switch]$arrayOutput) {
   $r = new-Object System.Security.Cryptography.RijndaelManaged
   $pass = [Text.Encoding]::UTF8.GetBytes($Passphrase)
   $salt = [Text.Encoding]::UTF8.GetBytes($salt)
 
   $r.Key = (new-Object Security.Cryptography.PasswordDeriveBytes $pass, $salt, "SHA1", 5).GetBytes(32) #256/8
   $r.IV = (new-Object Security.Cryptography.SHA1Managed).ComputeHash( [Text.Encoding]::UTF8.GetBytes($init) )[0..15]
   
   $c = $r.CreateEncryptor()
   $ms = new-Object IO.MemoryStream
   $cs = new-Object Security.Cryptography.CryptoStream $ms,$c,"Write"
   $sw = new-Object IO.StreamWriter $cs
   $sw.Write($String)
   $sw.Close()
   $cs.Close()
   $ms.Close()
   $r.Clear()
   [byte[]]$result = $ms.ToArray()
   if($arrayOutput) {
      return $result
   } else {
      return [Convert]::ToBase64String($result)
   }
}

# Helper function to decrypt stuff, found it on the internet.
function Decrypt-String($Encrypted, $Passphrase, $salt="My Voice is my P455W0RD!", $init="Yet another key") {
   if($Encrypted -is [string]){
      $Encrypted = [Convert]::FromBase64String($Encrypted)
   }
 
   $r = new-Object System.Security.Cryptography.RijndaelManaged
   $pass = [System.Text.Encoding]::UTF8.GetBytes($Passphrase)
   $salt = [System.Text.Encoding]::UTF8.GetBytes($salt)
 
   $r.Key = (new-Object Security.Cryptography.PasswordDeriveBytes $pass, $salt, "SHA1", 5).GetBytes(32) #256/8
   $r.IV = (new-Object Security.Cryptography.SHA1Managed).ComputeHash( [Text.Encoding]::UTF8.GetBytes($init) )[0..15]
 
   $d = $r.CreateDecryptor()
   $ms = new-Object IO.MemoryStream @(,$Encrypted)
   $cs = new-Object Security.Cryptography.CryptoStream $ms,$d,"Read"
   $sr = new-Object IO.StreamReader $cs
   Write-Output $sr.ReadToEnd()
   $sr.Close()
   $cs.Close()
   $ms.Close()
   $r.Clear()
}

$myGetKey = ""
$nugetExeFilePath = Join-Path $PSScriptRoot "NuGet.exe"

Try {
    # Unfortunately this doesn't work in "Visual Studio Code" and we get exception...
    $pass = Read-Host -Prompt "Enter Password for MyGet.org pushing" -AsSecureString 
    # In order not to save MyGet password bare and naked, I have it encrypted (used Encrypt-String cmdlet, included above). User now has to manually enter password, or NuGets will not be deployed.
    $myGetKey = Decrypt-String "6Bry9kvf9goVeFMG0SgJRyESFGOoypOc9YU2ZMyhMaF+idoDJyDEfXyzcbo5pLSy" ( [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($pass)))
}
Catch {
    throw "Incorrect password."
}

$myGetSourceLink = "https://www.myget.org/F/light-conversion-simon-and-friends/auth/" + $myGetKey + "/api/v3/index.json"

& $nugetExeFilePath push $path -Source $myGetSourceLink -NonInteractive