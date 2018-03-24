<?php
	
	include 'ConfigurationYourNetworkTools.php';
	
	$lobby = $_GET["lobby"];	
	$userid = $_GET["userid"];
			
	ConsultRequestAllRooms($lobby, $userid);

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
     //  ConsultRequestAllRooms
     //-------------------------------------------------------------
     function ConsultRequestAllRooms($lobby_par, $userid_par)
     {
		$query_rooms_consult = "SELECT * FROM rooms WHERE lobby = $lobby_par AND completed = 0 ORDER BY created DESC";
		$result_rooms_consult = mysqli_query($GLOBALS['LINK_DATABASE'],$query_rooms_consult) or die("Query Error::ConsultRequestAllRooms::Failed to consult rooms");
	
		$output_packet = "";
		while ($row_prop_consult = mysqli_fetch_object($result_rooms_consult))
		{
			$id_room = $row_prop_consult->id;
			$name_room = $row_prop_consult->name;
			$players_room = $row_prop_consult->players;
			$ip_host_room = $row_prop_consult->host;
			$port_host_room = $row_prop_consult->port;
			$completed_room = $row_prop_consult->completed;
			$extradata_room = $row_prop_consult->extradata;
			
			$include_room = true;
			if ($lobby_par == 0)
			{
				$include_room = false;
				$players_array = explode(',',$players_room);
				foreach ($players_array as &$player_check)
				{
					if ($player_check == $userid_par)
					{
						$include_room = true;
					}
				}			
			}
			
			if ($include_room)
			{
				$line_prop = $id_room . $GLOBALS['PARAM_SEPARATOR'] .  $name_room . $GLOBALS['PARAM_SEPARATOR'] . $ip_host_room . $GLOBALS['PARAM_SEPARATOR'] . $port_host_room . $GLOBALS['PARAM_SEPARATOR'] . $extradata_room;
				
				$output_packet = $output_packet . $GLOBALS['LINE_SEPARATOR'] . $line_prop;
			}
		}
		
		print $output_packet;		
		
		mysqli_free_result($result_rooms_consult);
    }
	
?>
