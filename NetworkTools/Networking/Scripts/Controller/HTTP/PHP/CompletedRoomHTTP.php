<?php
	
	include 'ConfigurationYourNetworkTools.php';

	$room = $_GET["room"];

	UpdateRoomCompleted($room);
	
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
     //  UpdateRoomCompleted
     //-------------------------------------------------------------
     function UpdateRoomCompleted($room_par)
     {
		// UPDATE FREE MACHINE WITH NEW ROOM
		$query_update_room_completed = "UPDATE rooms SET completed = 1 WHERE id = $room_par";
		$result_update_room_completed = mysqli_query($GLOBALS['LINK_DATABASE'],$query_update_room_completed) or die("Query Error::UpdateRoomCompleted::Failed to udpate the room=".$query_update_room_completed);
		
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
