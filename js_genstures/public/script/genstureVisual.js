let data = verificationData;

let canv,ctx;
//let classColors = ["red", "blue", "yellow", "green", "cyan", "magenta", "pink", "orange", "purple", "azure", "aqua", "darkgrey"];
let classColors = ["255,0,0", "0,0,255", "255, 215, 0", "0,255,0", "143, 188, 143", "139, 0, 139", "255, 20, 147", "255, 140, 0", "255, 250, 240", "250, 235, 215", "0, 255, 255", "169, 169, 169"];
let activeClass = 0;

let samples = [];



window.onload = function(){
    canv = document.getElementById('gc');
    ctx = canv.getContext("2d");
    
    window.addEventListener("mouseup", onMouseUp, false);

    drawBlackScreen();
    showData();
    drawLegend();
}
function drawBlackScreen(){
    ctx.fillStyle = "black";
    ctx.fillRect(0,0,canv.width, canv.height);
}

function showData(){
    console.log("Data entries", data.length);
    let rows = Math.floor(canv.width / RECT_WIDTH);
    let cols = Math.floor(canv.height / RECT_HEIGHT);

    let i = 0;
    for(let row = 0; row < rows && i < data.length; row++){
        for(let col = 0; col < cols && i < data.length; col++){
            drawFigure(data[i], col * RECT_WIDTH + 2, row*RECT_HEIGHT + 2);
            i++;
            samples.push({
                id: i,
                xStart: col * RECT_WIDTH + 2,
                yStart: row*RECT_HEIGHT + 2,
                xEnd: col * RECT_WIDTH + 2 + RECT_WIDTH - 1, 
                yEnd: row*RECT_HEIGHT + 2 + RECT_HEIGHT - 1,
                type: -1
            });
        }
    }
}

function onMouseUp(e){
    // Получить координаты мыши 
    let x,y;    
    // Для браузера IE
    if (document.all)  { 
        x = event.x + document.body.scrollLeft; 
        y = event.y + document.body.scrollTop; 
        // Для остальных браузеров
    } else {
        x = e.pageX; // Координата X курсора
        y = e.pageY; // Координата Y курсора
    }

    if(e.button == 0){ // левый клик
        
        // клик в области легенды
        if(x >= canv.width - 120){
            
            let u = Math.floor( (y) / RECT_HEIGHT);
            if(u >=0 && u < classColors.length){
                console.log("new class",u, y);
                activeClass = u;
                drawActiveClass();
            }
        }
        else {
            setClassToRect(x,y, activeClass);
        }
    }
    else if(e.button == 2){// правый клик
        var data = encode( JSON.stringify(samples, null, 4) );

        var blob = new Blob( [ data ], {
            type: 'application/octet-stream'
        });
        
        url = URL.createObjectURL( blob );
        var link = document.createElement( 'a' );
        link.setAttribute( 'href', url );
        link.setAttribute( 'download', 'y.js' );
        
        var event = document.createEvent( 'MouseEvents' );
        event.initMouseEvent( 'click', true, true, window, 1, 0, 0, 0, 0, false, false, false, false, 0, null);
        link.dispatchEvent( event );
    }
}

function encode( s ) {
    var out = [];
    for ( var i = 0; i < s.length; i++ ) {
        out[i] = s.charCodeAt(i);
    }
    return new Uint8Array( out );
}



function setClassToRect(x, y, cls){
    let xR = Math.floor(x / RECT_WIDTH) * RECT_WIDTH;
    let yR = Math.floor(y / RECT_HEIGHT) * RECT_HEIGHT;

    ctx.fillStyle = `rgba(${classColors[cls]}, 0.3)`;
    ctx.fillRect(xR,yR, RECT_WIDTH, RECT_HEIGHT);

    samples.filter((e)=> e.xStart <= x && e.xEnd >= x && e.yStart <= y && e.yEnd >= y).forEach(e=>e.type = activeClass);
}

function drawLegend(){
    for(let i in classColors){
        ctx.fillStyle = `rgba(${classColors[i]}, 0.7)`;
        ctx.fillRect(canv.width - 120, i * RECT_HEIGHT + 5, RECT_WIDTH, RECT_HEIGHT);

    }
}

function drawActiveClass(){
    // 
    ctx.fillStyle = "black";
    ctx.fillRect(canv.width - 120, canv.height - RECT_HEIGHT - 20, RECT_WIDTH, RECT_HEIGHT);
    ctx.fillStyle = `rgba(${classColors[activeClass]}, 0.7)`;
    ctx.fillRect(canv.width - 120, canv.height - RECT_HEIGHT - 20, RECT_WIDTH, RECT_HEIGHT);
}

function drawFigure(figureCords, offsetX, offsetY){

    ctx.strokeStyle = "#2bd647";
    ctx.lineWidth = 1;
    ctx.beginPath();
    let i = 0;
    
    
    ctx.moveTo(figureCords[i].x + offsetX, figureCords[i].y + offsetY);
    for(i; i< figureCords.length; i++){
        ctx.lineTo(figureCords[i].x + offsetX, figureCords[i].y + offsetY);        
        ctx.stroke();
    }
    ctx.closePath();

     // Получить обрамляющий прямоугольник 
     /* let minX = figureCords[0].x; 
     let maxX = minX;
     let minY = figureCords[0].y; 
     let maxY = minY;
     figureCords.forEach((e)=> {
         minX = e.x < minX ? e.x : minX;
         maxX = e.x > maxX ? e.x : maxX;
         minY = e.y < minY ? e.y : minY;
         maxY = e.y > maxY ? e.y : maxY;
     }); */
     
     // Нарисовать обрамляющий прямоугольник 
    ctx.beginPath();
    ctx.strokeStyle = "red";
    ctx.lineWidth = 1;
    //ctx.strokeRect(minX, minY,maxX - minX, maxY - minY);
    ctx.strokeRect(offsetX - 2, offsetY - 2, RECT_WIDTH - 1, RECT_HEIGHT - 1);
    ctx.stroke();
    ctx.closePath();
}