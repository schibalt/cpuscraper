
				<!--
                    /**
                    * DHTML email validation script. Courtesy of SmartWebby.com (http://www.smartwebby.com/dhtml/)
                    */

                    function echeck(str) {

                        var at = "@";
                        var dot = ".";
                        var lat = str.indexOf(at);
                        var lstr = str.length;
                        var ldot = str.indexOf(dot);
                        if (str.indexOf(at) == -1) {
                            //alert("Sorry. That's not a valid email address.");
                            return false
                        }

                        if (str.indexOf(at) == -1 || str.indexOf(at) == 0 || str.indexOf(at) == lstr) {
                            //alert("Sorry. That's not a valid email address.");
                            return false;
                        }

                        if (str.indexOf(dot) == -1 || str.indexOf(dot) == 0 || str.indexOf(dot) == lstr) {
                            //alert("Sorry. That's not a valid email address.");
                            return false;
                        }

                        if (str.indexOf(at, (lat + 1)) != -1) {
                            //alert("Sorry. That's not a valid email address.");
                            return false;
                        }

                        if (str.substring(lat - 1, lat) == dot || str.substring(lat + 1, lat + 2) == dot) {
                            //alert("Sorry. That's not a valid email address.");
                            return false;
                        }

                        if (str.indexOf(dot, (lat + 2)) == -1) {
                            //alert("Sorry. That's not a valid email address.");
                            return false;
                        }

                        if (str.indexOf(" ") != -1) {
                            //alert("Sorry. That's not a valid email address.");
                            return false;
                        }

                        return true;
                    }

                    function HotDealValidateForm() {
                        var emailID = document.getElementById('deals-email');

                        if ((emailID.value == null) || (emailID.value == "")) {
                            //alert("Please enter an email address.");
                            //emailID.focus();
                          //  return false;
                            
                            document.emailform.P.value = emailID.value;
                            return true;
                            

                        }
                        //if (echeck(emailID.value) == false) {
                        //    emailID.value = "";
                        //    emailID.focus();
                        //    return false;
                        //}
                        
                        if (emailID.value == "Email Address") 
                        {
                            emailID.value = "";
                        }
                        document.emailform.P.value = emailID.value;
                        return true;
                    }
				  // -->
                    

                    function ContactValidateForm() {
                        var femail = document.getElementById('input[email]');
                        document.getElementById('name').style.display = "none";
                        document.getElementById('problem').style.display = "none";
                        document.getElementById('subject').style.display = "none";
                         document.getElementById('email').style.display = "none";
                        
                        var doreturn = true;
                        
                        if (document.getElementById('input[name]').value == null || document.getElementById('input[name]').value == "") {
                            doreturn = false;

                            document.getElementById('name').style.display = "block";
                        }
                        
                         if (document.getElementById('textarea').value == null || document.getElementById('textarea').value == "") {
                            doreturn = false;

                            document.getElementById('problem').style.display = "block";
                        }
                        
                        
                        if (document.getElementById('input[subject]').selectedIndex == 0) {
                            doreturn = false;

                            document.getElementById('subject').style.display = "block";
                        }

                        if ((femail.value == null) || (femail.value == "") || echeck(femail.value) == false) {
                            

                            doreturn = false;

                            document.getElementById('email').style.display = "block";
                           
                            

                        }
                        
                        if (doreturn) {
                            return true;
                        } else {
                            return false;
                        }
                        
                    }
                    
	