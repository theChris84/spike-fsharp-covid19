# spike-fsharp-covid19
Spike to try to learn F#, Fable, F# Fake, Farmer


#Docker cli 
| CLI-CMD | Description |
| ----------- | ----------- |
| docker build -t covid-19-image  -f .\Dockerfile . | Build a docker container by specifing a repo (-t) name and docker file (-f) |
| docker run -p 5000:5000 --rm -it covid-19-image /bin/bash |  run a docker container -p port --rm remove container after stop -it interactive terminal  |