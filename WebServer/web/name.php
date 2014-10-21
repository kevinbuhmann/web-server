<html>
	<head>
		<title>Name</title>
	</head>
	<body>
		<h1>
			Hello, <?php echo $_SERVER["REQUEST_METHOD"] == "POST" ? $_POST["name"] : $_GET["name"]; ?>!
		</h1>
	</body>
</html>