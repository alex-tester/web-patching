enable-psremoting -force -confirm:$false

$s = new-pssession -computername localhost
if($?)
{
	$exitcode = 0
}
else
{
	$exitcode = 1
}

<#
Enable-WSManCredSSP -Role client -DelegateComputer * -Force
if($?)
{
	$exitcode = 0
}
else
{
	$exitcode = 1
}

Enable-WSManCredSSP -Role Server -Force
if($?)
{
	$exitcode = 0
}
else
{
	$exitcode = 1
}


#>
exit $exitcode