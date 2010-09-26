@echo off
rem Genera primero las dependencias
rem (los niveles superiores de las jerarquías)
rem y va descendiendo hacia niveles inferiores
set ETLGEN=.\bin\Debug\ETLGen.exe
set CONFIG=etc/Config.xml

echo Dependencias:
%ETLGEN% 100 data/edificio.xml %CONFIG%

echo Dependencias: edificio
%ETLGEN% 100 data/aula.xml %CONFIG%

echo Dependencias: edificio
%ETLGEN% 100 data/departamento.xml %CONFIG%

echo Dependencias: departamento
%ETLGEN% 100 data/area.xml %CONFIG%

echo Dependencias:
%ETLGEN% 100 data/examen.xml %CONFIG%

echo Dependencias:
%ETLGEN% 100 data/pais.xml %CONFIG%

echo Dependencias: pais
%ETLGEN% 100 data/provincia.xml %CONFIG%

echo Dependencias: provincia
%ETLGEN% 100 data/ciudad.xml %CONFIG%

echo Dependencias: ciudad
%ETLGEN% 100 data/colegioBUP.xml %CONFIG%

echo Dependencias: ciudad
%ETLGEN% 100 data/colegioCOU.xml %CONFIG%

echo Dependencias: ciudad
%ETLGEN% 100 data/colegioEGB.xml %CONFIG%

echo Dependencias: ciudad
%ETLGEN% 100 data/colegioFP.xml %CONFIG%

echo Dependencias: colegioBUP, colegioCOU, colegioEGB, colegioFP, ciudad
%ETLGEN% 100 data/persona.xml %CONFIG%

echo Dependencias: persona
%ETLGEN% 100 data/expediente.xml %CONFIG%

echo Dependencias: expediente
%ETLGEN% 100 data/matricula.xml %CONFIG%

echo Dependencias: pais
%ETLGEN% 100 data/autor.xml %CONFIG%

echo Dependencias:
%ETLGEN% 100 data/biblioteca.xml %CONFIG%

echo Dependencias: autor, biblioteca
%ETLGEN% 100 data/libro.xml %CONFIG%

echo Dependencias:
%ETLGEN% 100 data/doctorado.xml %CONFIG%

echo Dependencias: area, doctorado
%ETLGEN% 100 data/carrera.xml %CONFIG%

echo Dependencias: carrera, area, ciudad
%ETLGEN% 100 data/profesor.xml %CONFIG%

echo Dependencias: area, carrera
%ETLGEN% 100 data/asignatura.xml %CONFIG%

echo Dependencias:
%ETLGEN% 100 data/franjahoraria.xml %CONFIG%

echo Dependencias: matricula, examen, asignatura, fecha
%ETLGEN% 50000 data/abstencion.xml %CONFIG%

echo Dependencias: matricula, aula, franjahoraria, asignatura, fecha, profesor
%ETLGEN% 100000 data/asistencia.xml %CONFIG%

echo Dependencias: matricula, asignatura, examen, fecha, profesor
%ETLGEN% 50000 data/evaluacion.xml %CONFIG%

echo Dependencias: evaluacion, libro
%ETLGEN% 100000 data/librosestudiados.xml %CONFIG%

echo Dependencias: abstencion, libro
%ETLGEN% 100000 data/librosprestados.xml %CONFIG%
