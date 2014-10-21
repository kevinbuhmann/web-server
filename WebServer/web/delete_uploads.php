<?php
	$files = glob("./upload/*");
	foreach ($files as $filename)
	{
		if (($filename != ".blank") && is_file($filename))
			unlink($filename);
	}
	
    header("Location: /php_example.php");
?>