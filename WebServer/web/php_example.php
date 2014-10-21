<html>
<head>
	<title>PHP Example</title>
</head>
<body>
	<h1>
		PHP Example!</h1>
	<div style="float: left;">
		<fieldset>
			<legend>Simple GET Form</legend>
			<form method="get" action="name.php">
			    <label>
				    Name:
				    <input type="text" name="name" /></label>
			    <input type="submit" value="Submit" />
			</form>
		</fieldset>
		<br />
		<fieldset>
			<legend>Simple POST Form</legend>
			<form method="post" action="name.php">
			    <label>
				    Name:
				    <input type="text" name="name" /></label>
			    <input type="submit" value="Submit" />
			</form>
		</fieldset>
	</div>
	<div style="float: left;">
		<fieldset>
			<legend>Sign the Guestbook!</legend>
			<form method="post" action="sign_guestbook.php">
			    <label>
				    Name:
				    <input type="text" name="name" /></label>
			    <input type="submit" value="Sign!" />
			</form>
		</fieldset>
		<br />
		<fieldset>
			<legend>Guestbook</legend>
			<?php
                if (file_exists("guestbook.txt"))
                {
                    echo str_replace("\n", "<br />", file_get_contents("guestbook.txt"));
                    echo "<br /><a href='clear_guestbook.php'>Clear Guestbook</a>";
                }
            ?>
	</div>
	<div style="float: left;">
		<fieldset>
			<legend>Upload a File!</legend>
			<form action="upload_file.php" method="post" enctype="multipart/form-data">
                <input type="file" name="file" id="file">
                <input type="submit" name="submit" value="Upload!">
            </form>
		</fieldset>
		<br />
		<fieldset>
			<legend>Uploads</legend>
			<?php
				$file_exists = false;
				$files = glob("./upload/*");
				foreach ($files as $filename)
				{
					if (($filename != ".blank") && is_file($filename))
					{
						$file_exists = true;
						echo "<a target='_blank' href='" . $filename . "'>" . str_replace("./upload/", "", $filename) . "</a><br />";
					}
				}
				
				if ($file_exists)
                    echo "<br /><a href='delete_uploads.php'>Delete Uploads</a>";
			?>
	</div>
</body>
</html>
