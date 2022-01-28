// /import {loadImage} from './loaders.js'
let canv,ctx; 
let backImage

window.onload = function(){
    init();

    level1();
}

function init(){
    canv = document.getElementById('gc');
    ctx = canv.getContext("2d");
}

function level1(){
    let pBackground = loadImage('./img/daylight-03.jpg');
    let pPlayer = loadImage('./img/2D-Game-Minotaur-Character-Sprites3.png');

    Promise.all([pBackground, pPlayer]).then(values => {
        ctx.drawImage(values[0], 0,0);

        ctx.drawImage(values[1], 50, 500,
             168, 168,
            137,40,
            168,168);
    });

}

//const playerSprite = new SpriteSheet(image);