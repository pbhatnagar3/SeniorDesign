var bouncespeed : float = 0.002f;
var bouncing = true;
var itemBounceUp = true;
var initialY = 0;
var isColliding = false;
initialY = this.transform.position.y;

function Start() {
	itembounce();
}

function Update () {
	if(!isColliding){
 		if (itemBounceUp == true){
 			this.transform.position.y = (this.transform.position.y + bouncespeed) ;
 		}
 		else if (itemBounceUp == false){
 			this.transform.position.y -= bouncespeed;

 		}
 	} else {
 		this.transform.position.y = this.transform.position.y - 0.01 ;
 		this.transform.position.y = this.transform.position.y + 0.03 ;
 	}
}

function itembounce () {

	//Debug.Log("in itembounce");
	while(bouncing) {
		yield WaitForSeconds(.7);
		itemBounceUp = false;
		yield WaitForSeconds(1.2);
		itemBounceUp = true;
	}
}

function OnCollisionEnter(collision : Collision) {
	isColliding = true;
	Debug.Log("hi there Enter");
}

function OnCollisionExit(collisionInfo : Collision){
	isColliding = false;
	Debug.Log("hi there Out");
}