$response = [System.IO.MemoryMappedFiles.MemoryMappedFile]::CreateOrOpen("Vvec.Prj", 1024)

try
{
	C:\dev\eviltobz\Vvec.Cli\Vvec.Prj\bin\Debug\net8.0\Vvec.Prj.exe $args

	$accessor = $response.CreateViewAccessor();
	try
	{
		$length = $accessor.ReadInt16(0);
		if($length -ne 0)
		{
			$buffer = New-Object System.byte[] $length
			$_ = $accessor.ReadArray(2, $buffer, 0, $length)
			$cd = [System.Text.Encoding]::UTF8.GetString($buffer, 0, $length);

			cd $cd
		}
	}
	finally
	{
		$accessor.Dispose()
	}
}
finally
{
	$response.Dispose()
}
