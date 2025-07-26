// ============================================================
// Jans Code
// ============================================================


// #include <Servo.h>

// Servo myservo;

// String reciveDataRaw;
// String LastData;

// int servoPin = 13;

// // the setup function runs once when you press reset or power the board
// void setup() {
//   // initialize digital pin LED_BUILTIN as an output.
//   myservo.attach(servoPin);
//   //pinMode(LED_BUILTIN, OUTPUT);
//   Serial.begin(9600);

//   /*while (!Serial) {
//   digitalWrite(LED_BUILTIN, HIGH);
//   delay(1000);
//   digitalWrite(LED_BUILTIN, LOW);
//   delay(100);
//   digitalWrite(LED_BUILTIN, HIGH);
//   delay(100);
//   digitalWrite(LED_BUILTIN, LOW);
//   }
//   digitalWrite(LED_BUILTIN, HIGH);
//   delay(5000);
//   digitalWrite(LED_BUILTIN, LOW);
//   delay(1000); */


// }

// // the loop function runs over and over again forever
// void loop() {
//   reciveDataRaw = Serial.readString();

//   float ServoValue = 0;//reciveDataRaw.toFloat();
//   /*if(reciveDataRaw == "testON") {
//     digitalWrite(13, HIGH);
//   }
//   else {
//     digitalWrite(13, LOW);
//   }*/
//   //delay(1000);


//   //myservo.write(ServoValue);                  // sets the servo position according to the scaled value

//   delay(15);

//   myservo.write(95);

//   //delay(5000);

//   Serial.println(reciveDataRaw);
//   //digitalWrite(LED_BUILTIN, HIGH);  // turn the LED on (HIGH is the voltage level)
//   //delay(1000);                      // wait for a second
//   //digitalWrite(LED_BUILTIN, LOW);   // turn the LED off by making the voltage LOW
//   //delay(1000);                      // wait for a second
// }


// ============================================================
// Max Code
// ============================================================

void setup() {
  pinMode(LED_BUILTIN, OUTPUT);
  Serial.begin(9600);

  while (!Serial) {
    digitalWrite(LED_BUILTIN, HIGH);
    delay(1000);
    digitalWrite(LED_BUILTIN, LOW);
    delay(100);
    digitalWrite(LED_BUILTIN, HIGH);
    delay(100);
    digitalWrite(LED_BUILTIN, LOW);
  }

  digitalWrite(LED_BUILTIN, HIGH);
  delay(5000);
  digitalWrite(LED_BUILTIN, LOW);
  delay(1000);
}

// the loop function runs over and over again forever
void loop() {
  if (Serial.available() > 0) {
    String receivedChar = Serial.readStringUntil('\n');

    if (receivedChar == "ON") {
      digitalWrite(LED_BUILTIN, HIGH);
    } else if (receivedChar == "OFF") {
      digitalWrite(LED_BUILTIN, LOW);
    }

    Serial.println(receivedChar);
  }

  //delay(15);
}