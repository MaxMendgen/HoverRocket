#define MAINPIN 9

int highPulse = 1000; 
int wholePulse = 20000;

bool reflect = false;
int minHighPulse = 1000;
int maxHighPulse = 2000;
int highPulseAddition = 10;

long int startTime = 0;
long int cycleTime = 0;
long int lastCycleTime = 0;

void setup() {
  startTime = micros();
}

void loop() {
  SawWave();
  Pulse();
}

void SawWave() {
  if (cycleTime > lastCycleTime) {
    if (reflect) {
      highPulse -= highPulseAddition;
      if (highPulse <= minHighPulse)
        reflect = false;
    } else {
      highPulse += highPulseAddition;
      if (highPulse >= maxHighPulse)
        reflect = true;
    }

    lastCycleTime = cycleTime;
  }
}

void Pulse() {
  highPulse = constrain(highPulse, 1000, 2000);

  long int currentTime = micros();
  long int deltaTime = currentTime - startTime;

  if (deltaTime > highPulse) {
    digitalWrite(MAINPIN, LOW);
  } 
  if (deltaTime > wholePulse) {
    digitalWrite(MAINPIN, HIGH);
    cycleTime += 1;
    startTime = micros();
  }
}
