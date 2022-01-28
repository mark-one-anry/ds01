let istraining = false;  // находимся ли сейчас в режиме обучения
let lossX = [];
let lossY = [];
let accY = [];


/* PLAY SECTION */


/******
 * Мои данные - это изображения размером RECT_WIDTH x RECT_HEIGHT
 * Изображения представлены во входном JSON объекте не как настоящие изображения, а только как координаты точек, в которых есть поставленный пиксель
 * Для работы нейросети необходимо преобразовать входные данные
 * Каждое изображение - это матрица 75x55 или вектор из 1425 значений. В каждом элементе 0 или 1
 * Это делает функция convertDataToVector (для преобразования в вектор 1425)
 * И convertDataToMatrix
 * Из этого выражения можно сделать 2d тезор с shape [RECT_HEIGHT, RECT_WIDTH] (потому что сначала количество строк)
 * Кроме того, необходимо добавить количество образцов (изоборежений) data.length
 *******/

// Собираем данные для обучения воедино 
let data = [...circlesData,...verticalLineData,...infinityData,...victoryData];
let trainLabels = [...circlesLabels,...verticalLineLabels,...infinityLables,...victoryLabels];

// Конвертация входных данных (X)
let inputData = new Array();
for(let i in data){
    //inputData.push(convertDataToVector(data[i]));
    inputData.push(convertDataToMatrix(data[i]));
}
let xs = tf.tensor(inputData, [data.length, RECT_HEIGHT, RECT_WIDTH], 'float32');
xs.reshape([data.length, RECT_HEIGHT, RECT_WIDTH, 1]);

// Конвертация выходных данных (Y)
// Классы для тренировочных данных 
// из одной цифры в JSON объекте необходимо сделать вектор у которого будет установлен атрибут
let ys = [];
trainLabels.forEach((e)=> {
    let ar = [];
    for(let i = 0; i < NUM_OUTPUT_CLASSES; i++){
        ar[i] = i == e.type ? 1 : 0;
    }
    ys.push(ar);
});
ys = tf.tensor(ys, [data.length, NUM_OUTPUT_CLASSES], 'float32');


// Подготовка тестовых данных 
let testDataArray = new Array();
for(let i in testData){
    testDataArray.push(convertDataToMatrix(testData[i]));
}
let testX = tf.tensor(testDataArray, [testData.length, RECT_HEIGHT, RECT_WIDTH], 'float32');
testX = testX.reshape([testData.length, RECT_HEIGHT, RECT_WIDTH,1]);

// Классы для тестовых данных 
let testY = [];
testLabels.forEach((e)=> {
    let ar = [];
    for(let i = 0; i < NUM_OUTPUT_CLASSES; i++){
        ar[i] = i == e.type ? 1 : 0;
    }
    testY.push(ar);
});
testY = tf.tensor(testY, [testData.length, NUM_OUTPUT_CLASSES], 'float32');

let model = buildModel();

train(model, xs, ys, testX, testY)
    .then(
        ()=>{
            console.log("Training complete");
            validateModel();

            model.save('downloads://genstureRecognizer');
        }
    );


//train();


// преобразовать растр в вектор 
function convertDataToVector(d){
    let v = [];
    for(let i = 0; i< RECT_HEIGHT; i++){
        for(let j = 0; j < RECT_WIDTH; j++){
            v.push(d.filter(e=>parseInt(e.x) == j && parseInt(e.y) == i).length > 0 ? 1 : 0);
        }
    }

    let ch = v.filter(e => e>0).length;
    return v;
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

function buildModel() {
    // Базовая модель
    let md = tf.sequential();
    // добавим первый свёрточный слой 
    const layer1 = tf.layers.conv2d({
        inputShape: [RECT_HEIGHT, RECT_WIDTH, 1],
        kernelSize: 5, // размер скользящего окна 5х5
        filters: 8,
        strides: 1, // шаг скользящего окна
        activation: 'relu',
        kernelInitializer: 'varianceScaling'
    });    
    md.add(layer1);
    // Магия 
    // The MaxPooling layer acts as a sort of downsampling using max values
    // in a region instead of averaging.  
    md.add(tf.layers.maxPooling2d({poolSize: [2, 2], strides: [2, 2]}));    
    md.add(tf.layers.conv2d({
        kernelSize: 5,
        filters: 16,
        strides: 1,
        activation: 'relu',
        kernelInitializer: 'varianceScaling'
      }));
    md.add(tf.layers.maxPooling2d({poolSize: [2, 2], strides: [2, 2]}));
    // Now we flatten the output from the 2D filters into a 1D vector to prepare
    // it for input into our last layer. This is common practice when feeding
    // higher dimensional data to a final classification output layer.
    md.add(tf.layers.flatten());

     // Our last layer is a dense layer which has 10 output units, one for each
    // output class (i.e. 0, 1, 2, 3, 4, 5, 6, 7, 8, 9).
    
    md.add(tf.layers.dense({
        units: NUM_OUTPUT_CLASSES,
        kernelInitializer: 'varianceScaling',
        activation: 'softmax'
    }));
    const optimizer = tf.train.adam();
    md.compile({
        optimizer: optimizer,
        loss: 'categoricalCrossentropy',
        metrics: ['accuracy'],
    });


  
    return md
}

// Обучение модели 
async function train(model, trainX, trainY, testX, testY){
    // количество изображений для обучения и проверки 
    const TRAIN_DATA_SIZE = 80;
    const TEST_DATA_SIZE = 17;
    const BATCH_SIZE = 97;

    trainX = trainX.reshape([data.length, RECT_HEIGHT, RECT_WIDTH, 1]);
    testX = testX.reshape([testData.length, RECT_HEIGHT, RECT_WIDTH, 1]);
    console.log('trainX shape', trainX.shape);
    console.log('testX shape', testX.shape);

    return model.fit(trainX, trainY, {
        batchSize: BATCH_SIZE,
        validationData: [testX, testY],
        epochs: 100,
        shuffle: true,
        callbacks: {
            onEpochEnd: (epoch, logs) => {
                console.log(epoch);
                lossY.push(logs.val_loss.toFixed(2));
                accY.push(logs.val_acc.toFixed(2));
                lossX.push(lossX.length + 1);
                console.log('Loss: ' + logs.loss.toFixed(5));
            },
            onBatchEnd: async (batch, logs) => {
                await tf.nextFrame();
            },
            onTrainEnd: () => {
                istraining = false;
                console.log('fit finished');
            },
        }    
    });
}

function validateModel(){
    // Конвертация входных данных (X)    
    let inputData = new Array();
    // Преобразовать данные в матрицу
    for(let i in verificationData){        
        inputData.push(convertDataToMatrix(verificationData[i]));
    }
    // из матрицы в тензор
    
    let verificationTensor = tf.tensor(inputData, [verificationData.length, RECT_HEIGHT, RECT_WIDTH], 'float32');
    verificationTensor = verificationTensor.reshape([verificationData.length, RECT_HEIGHT, RECT_WIDTH, 1]);
    let res = model.predict(verificationTensor);
    
    res = res.reshape([verificationData.length, NUM_OUTPUT_CLASSES]);
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

    //debugger;
    for(let i in resData[0]){
        let prob = Math.max(...resData[0][i]);
        let cls = resData[0][i].indexOf(prob);
        console.log(`Sample ${i} got class ${cls} with probability ${prob}`); 
    }
    // debugger;
    //console.log(`Sample ${i} got class ${resData.indexOf(prob)} with probability ${prob}`); 

}
