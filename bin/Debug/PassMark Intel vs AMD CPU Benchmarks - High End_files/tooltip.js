var mouse_event_timer;

/* STAD - "Show Tip After Delay */
function STAD( event, rank, samples, cores, logicals, extra, extra2 ) {
	var xpos = event.clientX;
	var ypos = event.clientY;
	clearTimeout( mouse_event_timer );
	mouse_event_timer = setTimeout( function() { show_tip( xpos, ypos, rank, samples, cores, logicals, extra, extra2 ); }, 150 );
}

function pp_STAD( event, rating, rank, samples, cores, logicals, extra, speed ) {
	var xpos = event.clientX;
	var ypos = event.clientY;
	clearTimeout( mouse_event_timer );
	mouse_event_timer = setTimeout( function() { pp_show_tip( xpos, ypos, rating, rank, samples, cores, logicals, extra, speed ); }, 150 );
}

/* HTAD - "Hide Tip After Delay */
function HTAD( ) {
	clearTimeout( mouse_event_timer );
	mouse_event_timer = setTimeout( function() { hide_tip(); }, 150 );
}

function show_tip( xpos, ypos, rank, samples, cores, logicals, extra, extra2 ) {

	var tooltip = document.getElementById('tip');
	tooltip.style.left = (xpos+3) + 'px';
	tooltip.style.top = (ypos+3) + 'px';
	
	tooltip.innerHTML = "Rank: " + rank + "<br>";
	
	if( samples != -1 )
		tooltip.innerHTML += "Number of Samples: " + samples + "<br>";
	
	tooltip.innerHTML += "Number of Cores: " + cores;
	
	if( logicals > 1 )
	{
		tooltip.innerHTML += " (" + logicals + " logical per)<br>";
	}
	else
	{
		tooltip.innerHTML += "<br>";	
	}
	
	if( extra ) 
	{
		if( extra.indexOf('%') != -1 ) 
			tooltip.innerHTML += "Average Rating Increase: " + extra + "<br>";
		else
			tooltip.innerHTML += "Average Rating: " + extra + "<br>";
	} 
	
	if( extra2 )
	{
		tooltip.innerHTML += "Max TDP: " + extra2 + " W<br>";
	}
	
	tooltip.style.visibility='visible';
}


function pp_show_tip( xpos, ypos, rating, rank, samples, cores, logicals, extra, speed ) {

	var tooltip = document.getElementById('tip');
	tooltip.style.left = (xpos+3) + 'px';
	tooltip.style.top = (ypos+3) + 'px';
	
	
	
	
	if( extra ) 
	{
		if( extra.indexOf('%') != -1 ) {
			tooltip.innerHTML = "Rating: " + rating + "<br>Rank: " + rank + "<br>" + "Average Rating Increase: " + extra + "<br>";
		} else {
			tooltip.innerHTML = "Marketshare: " + rating + "%<br>Rank: " + rank + "<br>Rating: " + extra + "<br>";
		}		
	} else {
		tooltip.innerHTML = "Rating: " + rating + "<br>Rank: " + rank + "<br>";
	}
	
	if( samples != -1 )
		tooltip.innerHTML += "Number of Samples: " + samples + "<br>";
		
	tooltip.innerHTML += "Number of Cores: " + cores;
	
	if( logicals > 1 )
	{
		tooltip.innerHTML += " (" + logicals + " logical per)<br>";
	}
	else
	{
		tooltip.innerHTML += "<br>";	
	}
	
	
	tooltip.style.visibility='visible';
}


function hide_tip( ) {
	var tooltip = document.getElementById('tip');
	tooltip.style.visibility='hidden';
}
