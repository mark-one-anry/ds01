let canv,ctx; 
let oldCords; // координаты мыши из предыдущего события
let cordsArray = []; // массив координат рисуемой фигуры
let prevTime;   // время предыдущего измерения координат мыши
let prevCord;
let totalDistance = 0;  // общее пройденное расстояние во время рисования фигуры 
let prevDistance = 0;
let genstureMode = false;  // находимся ли в режиме рисования
let allGenstures = []; // массив всех нарисованных фигур 

let model;
let modelLoaded = false;

window.onload = function(){
    canv = document.getElementById('gc');
    ctx = canv.getContext("2d");

    init();
}

function loadModel(){
    const uploadJSONInput = document.getElementById('upload-json');
    const uploadWeightsInput = document.getElementById('upload-weights');
    tf.loadLayersModel(tf.io.browserFiles([uploadJSONInput.files[0], uploadWeightsInput.files[0]]))
        .then((resolve)=>{
            console.log("Model loaded");
            // debugger;
            model = resolve;
            modelLoaded = true;
        });
}

function convertDataToMatrix(d){
    let v = []; // shape should be RECT_WIDTH x RECT_HEIGHT x 1
    for(let i = 0; i< RECT_HEIGHT; i++){
        let row = [];
        for(let j = 0; j < RECT_WIDTH; j++){
            let px = d.filter(e=>parseInt(e.x) == j && parseInt(e.y) == i).length > 0 ? 1 : 0;
            //v.push([j,i,px]);
            //v[i,j] = px;
            row.push(px);
        }
        v.push(row);
    }
    return v;
}

function recognizeGensture(data){
    if(!modelLoaded)
        return null;
    // Преобразовать входные данные 
    let inputData = new Array();

    inputData.push(convertDataToMatrix(data));
    
    // Переделать в тензор
    let xs = tf.tensor(inputData, [1, RECT_HEIGHT, RECT_WIDTH], 'float32');
    xs= xs.reshape([1, RECT_HEIGHT, RECT_WIDTH, 1]);
    // Натравить модель
    let res = model.predict(xs);
    // Вернуть результат 
    res = res.reshape([1, NUM_OUTPUT_CLASSES]);
    let resData = res.dataSync();
    //convert to multiple dimensional array
    shape = res.shape;
    shape.reverse().map(a => {
        resData = resData.reduce((b, c) => {
            latest = b[b.length - 1]
            latest.length < a ? latest.push(c) : b.push([c])
            return b
        }, 
        [[]]
        )        
    });
   
    
    let r = {};
    //for(let i in resData[0]){
    r.probability = Math.max(...resData[0][0]);
    r.class = resData[0][0].indexOf(r.probability);
    return r;
    
    //}

}

function init(){
    drawBlackScreen();

    window.addEventListener("mousemove", mouseCoords, false);
    window.addEventListener("mousedown", onMouseDown, false);
    window.addEventListener("mouseup", onMouseUp, false);
    window.addEventListener("contextmenu", (e)=>e.preventDefault(), false);     
}

// Просто пример линии с градиентом
function drawSampleLine(){


    var grd = ctx.createLinearGradient(500,100, 600, 350);
    grd.addColorStop(0, "black");
    grd.addColorStop(0.35, "#69d2ff");
    grd.addColorStop(0.75, "#69d2ff");
    grd.addColorStop(1, "black");


    
    ctx.strokeStyle = grd;
    ctx.fillStyle = grd;
    ctx.lineWidth = 5;
    ctx.beginPath();
    ctx.moveTo(500,100);    
    ctx.lineTo(600,350);
    
    ctx.stroke();
    ctx.lineTo(550,450);
    ctx.stroke();
}

function drawBlackScreen(){
    ctx.fillStyle = "black";
    ctx.fillRect(0,0,canv.width, canv.height);
}


function onMouseDown(e){
    if(e.button == 2){ // Правая кнопка
        genstureMode = true;
        prevTime = performance.now();
        cordsArray = [];
        drawBlackScreen();
    }
}

function onMouseUp(e){
    if(e.button == 2 && genstureMode){ // Правая кнопка - закончить рисование фигуры
        genstureMode = false;
        finishGensture();
    }
    /* else if (e.button == 0 && !genstureMode){ // левая кнопка
        saveGenstures();
    } */
    
}

// Сохранение всех нарисованных фигур
function encode( s ) {
    var out = [];
    for ( var i = 0; i < s.length; i++ ) {
        out[i] = s.charCodeAt(i);
    }
    return new Uint8Array( out );
}

function saveGenstures(){
    console.log("Saving ", allGenstures.length + " images");

    var data = encode( JSON.stringify(allGenstures, null, 4) );

    var blob = new Blob( [ data ], {
        type: 'application/octet-stream'
    });
    
    url = URL.createObjectURL( blob );
    var link = document.createElement( 'a' );
    link.setAttribute( 'href', url );
    link.setAttribute( 'download', 'example.json' );
    
    var event = document.createEvent( 'MouseEvents' );
    event.initMouseEvent( 'click', true, true, window, 1, 0, 0, 0, 0, false, false, false, false, 0, null);
    link.dispatchEvent( event );
}

// Обработка нарисованной фигуры
function processGensture(){
    // Перерисовать фигуру зеленым цветом (типа определилась)
    drawBlackScreen();
    ctx.strokeStyle = "#2bd647";
    ctx.lineWidth = 4;
    ctx.beginPath();
    let i = 0;
    ctx.moveTo(cordsArray[i].x, cordsArray[i].y);
    for(i; i< cordsArray.length; i++){
        ctx.lineTo(cordsArray[i].x, cordsArray[i].y);        
        ctx.stroke();
    }
    ctx.closePath();
    // Провести обработку изображения
    preProcessImage(cordsArray);

}

// Обработка нарисованного изображения (сжатие и т.п.)
function preProcessImage(cords){
    const IMAGE_WIDTH = 70;
    const IMAGE_HEIGHT = 50;
    // Получить обрамляющий прямоугольник 
    let minX = cords[0].x; 
    let maxX = minX;
    let minY = cords[0].y; 
    let maxY = minY;
    cords.forEach((e)=> {
        minX = e.x < minX ? e.x : minX;
        maxX = e.x > maxX ? e.x : maxX;
        minY = e.y < minY ? e.y : minY;
        maxY = e.y > maxY ? e.y : maxY;
    });
    
    // Нарисовать обрамляющий прямоугольник 
    ctx.beginPath();
    ctx.strokeStyle = "red";
    ctx.strokeRect(minX, minY,maxX - minX, maxY - minY);
    ctx.stroke();
    ctx.closePath();

    // Найти середину
    let xCenter = minX + (maxX - minX)/2;
    let yCenter = minY + (maxY - minY)/2;
    let pCenter = {
        x: xCenter,
        y: yCenter
    };
    let pAngleBegin = {
        x: maxX,
        y: yCenter
    };
    // Найти коэффициент масштабирования
    let kX =  IMAGE_WIDTH / (maxX - minX);
    let kY =  IMAGE_HEIGHT / (maxY - minY);

    // Сделать координаты для нового смасштабированного изображения 
    let scaledCords = [];
    cords.forEach((e)=>{
        scaledCords.push({
            /* x: (e.x - xCenter) * kX + xCenter - ((xCenter - minX - IMAGE_WIDTH) > 0 ? (xCenter - minX - IMAGE_WIDTH) : 0),
            y: (e.y - yCenter) * kY + yCenter - ((yCenter - minY - IMAGE_HEIGHT) > 0 ? (yCenter - minY - IMAGE_WIDTH) : 0) */
            /* x: (e.x - xCenter) * kX  - ((xCenter - minX - IMAGE_WIDTH/2) > 0 ? (xCenter - minX - IMAGE_WIDTH/2) : 0),
            y: (e.y - yCenter) * kY  - ((yCenter - minY - IMAGE_HEIGHT/2) > 0 ? (yCenter - minY - IMAGE_WIDTH/2) : 0) */
            x: (e.x - xCenter) * kX  + IMAGE_WIDTH/2,
            y: (e.y - yCenter) * kY  + IMAGE_HEIGHT/2
        });
    });

    // Распознать нарисованную фигуру 
    if(modelLoaded){
        let rec = recognizeGensture(scaledCords);
        
        let genstureName = GENSTURE_CLASSES[rec.class];
        console.log(`You've drawed ${genstureName} with prob ${rec.probability}`); 
    }

    // Нарисовать новую фигуру 
    ctx.strokeStyle = "yellow";
    ctx.lineWidth = 3;
    ctx.beginPath();
    let i = 0;
    ctx.moveTo(scaledCords[i].x, scaledCords[i].y);
    for(i; i< scaledCords.length; i++){
        ctx.lineTo(scaledCords[i].x, scaledCords[i].y);
        ctx.stroke();
    }
    ctx.closePath();

    // Добавить обработанную фигуру в общий массив фигур для сохранения
    allGenstures.push(scaledCords);

}


function finishGensture(){
    console.log("gensture complete");
    processGensture();
    genstureMode = false;
    
    oldCords = undefined;
    return;
}

function mouseCoords(e) {
    if(!genstureMode){
        return;
    }
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


    // Если это не первое измерение
    if(typeof(oldCords) != "undefined"){
        // Нарисовать линию от старых координат к новым 
        ctx.strokeStyle = "#69d2ff";
        ctx.lineWidth = 4;
        ctx.beginPath();
        ctx.moveTo(oldCords.x, oldCords.y);
        ctx.lineTo(x,y);
        ctx.stroke();
        ctx.closePath();

        // Добавить координату в массив 
        cordsArray.push({x:x, y:y});
        // Обновить общее расстояние 
        let l = cordsArray.length - 1; 
        let newDistance = getVDistance(cordsArray[l-1], cordsArray[l]);
        totalDistance+=newDistance;
  

        // Если за последние TIMEмс пройдено меньше PX пикселей - заверить фигуру
        const TIME = 50;
        const PX = 5;
        if(performance.now() - prevTime > TIME){
            //let d = Math.sqrt( (x - oldCords.x)*(x - oldCords.x) + (y - oldCords.y)*(y - oldCords.y));
            let d = totalDistance - prevDistance;
            if(d < PX){
                // ВКЛЮЧИТЬ ДЛЯ ОГРАНИЧЕНИЯ ВРЕМЕНИ
                //finishGensture();
                //return;

            }
            else {
                prevTime = performance.now();
                prevDistance = totalDistance;
            }
        }

        // Обновить старые координаты 
        oldCords.x = x;
        oldCords.y = y;


        
    }
    else {
        ctx.moveTo(x,y);
        ctx.beginPath();
        
        prevCord = {
            x: x,
            y: y
        };
        
        oldCords = {
            x: x,
            y: y
        };
        cordsArray.push({x:x, y:y});
    }

   }

//


