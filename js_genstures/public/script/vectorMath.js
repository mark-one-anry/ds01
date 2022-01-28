// Евклидово расстояние между двумя точками на плоскости
function getVDistance(a,b){
    return Math.sqrt( (b.x-a.x)*(b.x-a.x) + (b.y-a.y)*(b.y-a.y) );
}

// Получить угол в радианах линии из точек a в b к оси X
function getVAngle(a,b){
    Math.atan( (b.x - a.x) / (a.y - b.y) );
}
