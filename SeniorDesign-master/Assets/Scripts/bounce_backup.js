var bouncespeed : float = 0.002f;
var bouncing = true;
var itemBounceUp = true;
var initialY = 0;
initialY = this.transform.position.y;

function Start() {
	itembounce();
}

function Update () {
 if (itemBounceUp == true){
 	this.transform.position.y = (this.transform.position.y + bouncespeed) ;
 }
 else if (itemBounceUp == false){
 	this.transform.position.y -= bouncespeed;

 }
}

function itembounce () {

	Debug.Log("in itembounce");
	while(bouncing) {
		yield WaitForSeconds(1.2);
		itemBounceUp = false;
		yield WaitForSeconds(1.2);
		itemBounceUp = true;
	}
}

function OnCollisionEnter(collision : Collision) {
		Debug.Log("hi there");
		}