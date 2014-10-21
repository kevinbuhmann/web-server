<?php
	if ($_SERVER["REQUEST_METHOD"] == "POST")
	{
		if (!file_exists("./upload/"))
			mkdir("./upload/", 0777, true);
	
		move_uploaded_file($_FILES["file"]["tmp_name"], "./upload/" . $_FILES["file"]["name"]);
	}
	
	header("Location: /php_example.php");
?>