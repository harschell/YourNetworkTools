<?php
	
	include 'ConfigurationYourNetworkTools.php';

	$hostid = $_GET["hostid"];
	$roomid = $_GET["roomid"];

	UpdateHostMachine($hostid);
	
	DeleteRoom($roomid);
	
    // Closing connection
    mysqli_close($GLOBALS['LINK_DATABASE']);
          
     //**************************************************************************************
     //**************************************************************************************
     //**************************************************************************************
     // FUNCTIONS
     //**************************************************************************************
     //**************************************************************************************
     //**************************************************************************************     

	 //-------------------------------------------------------------
     //  UpdateHostMachine
     //-------------------------------------------------------------
     function UpdateHostMachine($hostid_par)
     {
		// UPDATE FREE MACHINE WITH NEW ROOM
		$query_update_free_machine = "UPDATE machines SET rooms = rooms - 1 WHERE id = $hostid_par";
		$result_update_free_machine = mysqli_query($GLOBALS['LINK_DATABASE'],$query_update_free_machine) or die("Query Error::UpdateHostMachine::Failed to udpate the machines=".$query_update_free_machine);
		
		// UPDATE AND SEND CONFIRMATION BACK TO CLIENT
		if (mysqli_affected_rows($GLOBALS['LINK_DATABASE']) == 1)
		{
			return true;
		}
		else
		{
			return false;
		}
	 }

	 //-------------------------------------------------------------
     //  DeleteRoom
     //-------------------------------------------------------------
     function DeleteRoom($roomid_par)
     {
		// UPDATE FREE MACHINE WITH NEW ROOM
		$query_delete_room = "DELETE FROM rooms WHERE id = $roomid_par";
		$result_delete_room = mysqli_query($GLOBALS['LINK_DATABASE'],$query_delete_room) or die("Query Error::FreeHostMachine::Failed to delete room=".$query_delete_room);
		
		// UPDATE AND SEND CONFIRMATION BACK TO CLIENT
		if (mysqli_affected_rows($GLOBALS['LINK_DATABASE']) == 1)
		{
			return true;
		}
		else
		{
			return false;
		}
	 }
	 
	 

?>
