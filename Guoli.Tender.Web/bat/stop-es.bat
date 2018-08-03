FOR /F "tokens=5 delims= " %P IN ('netstat -ano ^| findstr :9300') DO TaskKill /PID %P /F

elasticsearch