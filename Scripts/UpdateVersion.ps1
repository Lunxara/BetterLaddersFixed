$csprojPath = "BetterLadders.csproj"
if (-not (Test-Path $csprojPath)) {
    Write-Error "$csproj not found"
    exit 1
}
$content = (Get-Content $csprojPath)
$currentVersion = [regex]::Match($content, '<Version>(\d+\.\d+\.\d+)<\/Version>').Groups[1].Value
Add-Type -AssemblyName System.Windows.Forms

$form = New-Object System.Windows.Forms.Form
$form.Text = "Input Box"
$form.Size = New-Object System.Drawing.Size(300,150)
$form.StartPosition = "CenterScreen"

$label = New-Object System.Windows.Forms.Label
$label.Location = New-Object System.Drawing.Point(10,20)
$label.Size = New-Object System.Drawing.Size(260,20)
$label.Text = "New version (blank for $currentVersion):"
$form.Controls.Add($label)

$textbox = New-Object System.Windows.Forms.TextBox
$textbox.Location = New-Object System.Drawing.Point(10,40)
$textbox.Size = New-Object System.Drawing.Size(260,20)

$textbox.Add_KeyDown({
    param($sender, $e)
    if ((-not ($e.KeyCode -match '\d|OemPeriod|Back|Left|Right|Ctrl|Shift|Del')) -or ($textbox.Text[$textbox.Text.Length - 1] -eq '.' -and $e.KeyCode -eq 'OemPeriod') -or ($textbox.Text.Split('.').Count-1 -eq 2 -and $e.KeyCode -eq 'OemPeriod')) {
        $e.SuppressKeyPress = $true
    }
})
$textbox.Add_TextChanged({
    $okButton.Enabled = $textbox.Text -match '^\d{1,2}\.\d{1,2}\.\d{1,2}$'
})

$form.Controls.Add($textbox)

# Create an OK button
$okButton = New-Object System.Windows.Forms.Button
$okButton.Location = New-Object System.Drawing.Point(75,80)
$okButton.Size = New-Object System.Drawing.Size(75,23)
$okButton.Text = "OK"
$okButton.DialogResult = [System.Windows.Forms.DialogResult]::OK
$okButton.Enabled = $false
$form.Controls.Add($okButton)

# Show the form
$result = $form.ShowDialog()

# Get the user input if the OK button is clicked
if ($result -eq [System.Windows.Forms.DialogResult]::OK) {
    if ([string]::IsNullOrWhiteSpace($textbox.Text)) {
        Write-Output "Keeping current version ($currentVersion)"
        exit 0
    } else {
        $newVersion = $textbox.Text
    }
} else {
    Write-Output "Version not selected, stopping build"
    exit 1
}

Write-Output "Updating version to: $newVersion"
$content.Replace("<Version>$currentVersion</Version>", "<Version>$newVersion</Version>") | Set-Content -Path $csprojPath
$manifestPath = '..\manifest.json'
(Get-Content -Path $manifestPath).Replace("`"$currentVersion`"", "`"$newVersion`"") | Set-Content -Path $manifestPath
[System.Windows.Forms.MessageBox]::Show("Add $newVersion to changelog", "Reminder", "OK", "Information")

$form.Dispose()