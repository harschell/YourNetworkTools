<?php

	header('Content-type: text/html; charset=utf-8');
		 
	// Connecting, selecting database
    $LINK_DATABASE = mysqli_connect("localhost", "root", "")
       or die("Could not connect");
    // print "Connected successfully<p>";
    mysqli_select_db($LINK_DATABASE, "yournetworktools") or die("Database Error::Could not select database)");

	// RESPONSES WILL INCLUDE SPECIAL CHARACTERS BECAUSE THEY CONTAIN USERS' WORDS
	mysqli_query ($LINK_DATABASE, "set character_set_client='utf8'"); 
	mysqli_query ($LINK_DATABASE, "set character_set_results='utf8'"); 
	mysqli_query ($LINK_DATABASE, "set collation_connection='utf8_general_ci'");

	// OFFICIAL NAME OF THE APPLICATION
	$OFFICIAL_NAME_APPLICATION_GLOBAL = " Your Network Tools ";
	
	$DEFAULT_MACHINE = "localhost";
	$DEFAULT_PORT = 8745;
	
	// SEPARATOR TOKENS USED IN HTTPS COMMUNICATIONS
	$PARAM_SEPARATOR = "<par>";
	$LINE_SEPARATOR = "<line>";
	

	//-------------------------------------------------------------
	//  GetCurrentTimestamp
	//-------------------------------------------------------------
	function GetCurrentTimestamp()
    {
		$datebeging = new DateTime('1970-01-01');
		$currDate = new DateTime();
		$diff = $datebeging->diff($currDate);
		$secs=$diff->format('%a') * (60*60*24);  //total days
		$secs+=$diff->format('%h') * (60*60);     //hours
		$secs+=$diff->format('%i') * 60;              //minutes
		$secs+=$diff->format('%s');                     //seconds
		return $secs;
    }
		
?>