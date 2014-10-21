<?php
    if ($_SERVER["REQUEST_METHOD"] == "POST")
    {
        date_default_timezone_set("America/Chicago");
        $signature = date("m-d-y g:i a") . ": " . $_POST["name"] . "\n";
        file_put_contents("guestbook.txt", $signature, FILE_APPEND | LOCK_EX);
	}
	
    header("Location: /php_example.php");
?>