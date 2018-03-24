<?php
	
	include 'ConfigurationYourNetworkTools.php';

	$lobby = $_GET["lobby"];
	$room = $_GET["room"];
	$players = $_GET["players"];
	$extradata = $_GET["extra"];

    // ++ CREATE NEW ROOM ++
	CreateNewRoom($lobby, $room, $players, $extradata);	
	
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
     //  GetHostMachine
     //-------------------------------------------------------------
     function GetHostMachine()
     {
		$query_machines = "SELECT * FROM machines ORDER BY rooms DESC";
		$result_machines = mysqli_query($GLOBALS['LINK_DATABASE'],$query_machines) or die("Query Error::CreateNewRoom::Select the maximum room number");

		if ($row_machines = mysqli_fetch_object($result_machines))		
		{
			$id_machine = $row_machines->id;
			$ip_machine = $row_machines->ip;
			$port_machine = $row_machines->port;
			$rooms_in_machine = $row_machines->rooms;
			
			$free_machine = array($id_machine, $ip_machine, $port_machine, $rooms_in_machine);
		
			mysqli_free_result($result_machines);
		
			return $free_machine;
		}		
		return null;
	 }

	 //-------------------------------------------------------------
     //  UpdateHostMachine
     //-------------------------------------------------------------
     function UpdateHostMachine($free_machine_id_par)
     {
		// UPDATE FREE MACHINE WITH NEW ROOM
		$query_update_free_machine = "UPDATE machines SET rooms = rooms + 1 WHERE id = $free_machine_id_par";
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
     //  CreateNewRoom
     //-------------------------------------------------------------
     function CreateNewRoom($lobby_par, $room_par, $players_par, $extradata_par)
     {
		$query_consult = "SELECT max(id) as maximumId FROM rooms";
		$result_consult = mysqli_query($GLOBALS['LINK_DATABASE'],$query_consult) or die("Query Error::CreateNewRoom::Select the maximum room number");
		$row_consult = mysqli_fetch_object($result_consult);
		$maxIdentifier = $row_consult->maximumId;
		mysqli_free_result($result_consult);
			
		$room_id_new = $maxIdentifier + 1;			
		
		$free_machine_address = GetHostMachine();
		$free_machine_id = $free_machine_address[0];
		$free_machine_ip = $free_machine_address[1];
		$free_machine_port = $free_machine_address[2];
		$free_machine_rooms = $free_machine_address[3];
		$current_time_room = GetCurrentTimestamp();
		
		$query_insert = "INSERT INTO rooms VALUES ($room_id_new, '$room_par', $lobby_par, '$players_par', '$free_machine_ip', $free_machine_port, 0, $current_time_room, '$extradata_par')";
		$result_insert = mysqli_query($GLOBALS['LINK_DATABASE'],$query_insert) or die("Query Error::CreateNewRoom::Insert rooms failed::$query_insert=" . $query_insert);
			
		if (mysqli_affected_rows($GLOBALS['LINK_DATABASE']) == 1)
		{
			if (UpdateHostMachine($free_machine_id))
			{
				print "true" . $GLOBALS['PARAM_SEPARATOR'] .  $room_id_new . $GLOBALS['PARAM_SEPARATOR'] .  $free_machine_ip . $GLOBALS['PARAM_SEPARATOR'] .  $free_machine_port . $GLOBALS['PARAM_SEPARATOR'] . $free_machine_id;
			}
			else
			{
				print "false";
			}
		}
		else
		{
			print "false";
		}
    }

?>
