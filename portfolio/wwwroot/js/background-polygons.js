
document.addEventListener("DOMContentLoaded", function () {

    const reducedMotion = window.matchMedia(
        "(prefers-reduced-motion: reduce)"
    ).matches;

    const finePointer = window.matchMedia(
        "(hover: hover) and (pointer: fine)"
    ).matches;

    if (reducedMotion || !finePointer) return;

    // CREATE BACKGROUND CANVAS
    const canvas = document.createElement("canvas");
    canvas.id = "background-polygons";

    document.body.prepend(canvas);

    const ctx = canvas.getContext("2d");

    let width;
    let height;
    let animationFrame;

    const cursor = {
        x: -1000,
        y: -1000
    };

    const polygons = [];
    const polygonCount = 35;

    // RESIZE CANVAS
    function resizeCanvas() {

        const pixelRatio = Math.min(window.devicePixelRatio || 1, 2);

        width = window.innerWidth;
        height = window.innerHeight;

        canvas.width = width * pixelRatio;
        canvas.height = height * pixelRatio;

        canvas.style.width = width + "px";
        canvas.style.height = height + "px";

        ctx.setTransform(pixelRatio, 0, 0, pixelRatio, 0, 0);
    }

    // CREATE RANDOM POLYGONS
    function createPolygons() {

        polygons.length = 0;

        for (let i = 0; i < polygonCount; i++) {

            const sides = Math.floor(Math.random() * 3) + 3;

            polygons.push({
                x: Math.random() * width,
                y: Math.random() * height,

                size: Math.random() * 35 + 20,
                sides: sides,

                rotation: Math.random() * Math.PI,
                rotationSpeed: (Math.random() - 0.5) * 0.002,

                driftX: (Math.random() - 0.5) * 0.25,
                driftY: (Math.random() - 0.5) * 0.25,

                offsetX: 0,
                offsetY: 0,

                opacity: Math.random() * 0.25 + 0.08
            });
        }
    }

    // DRAW A SINGLE POLYGON
    function drawPolygon(polygon) {

        const x = polygon.x + polygon.offsetX;
        const y = polygon.y + polygon.offsetY;

        ctx.beginPath();

        for (let i = 0; i < polygon.sides; i++) {

            const angle =
                (Math.PI * 2 / polygon.sides) * i +
                polygon.rotation;

            const pointX = x + Math.cos(angle) * polygon.size;
            const pointY = y + Math.sin(angle) * polygon.size;

            if (i === 0) {
                ctx.moveTo(pointX, pointY);
            } else {
                ctx.lineTo(pointX, pointY);
            }
        }

        ctx.closePath();

        ctx.strokeStyle = `rgba(255, 35, 55, ${polygon.opacity})`;
        ctx.lineWidth = 1;

        ctx.stroke();
    }

    // ANIMATION LOOP
    function animate() {

        ctx.clearRect(0, 0, width, height);

        polygons.forEach(polygon => {

            // SLOW NATURAL MOVEMENT
            polygon.x += polygon.driftX;
            polygon.y += polygon.driftY;
            polygon.rotation += polygon.rotationSpeed;

            // WRAP AROUND SCREEN
            if (polygon.x < -80) polygon.x = width + 80;
            if (polygon.x > width + 80) polygon.x = -80;

            if (polygon.y < -80) polygon.y = height + 80;
            if (polygon.y > height + 80) polygon.y = -80;

            // CURSOR REPULSION
            const dx = polygon.x - cursor.x;
            const dy = polygon.y - cursor.y;

            const distance = Math.sqrt(dx * dx + dy * dy);
            const interactionRadius = 180;

            let targetX = 0;
            let targetY = 0;

            if (distance < interactionRadius && distance > 0) {

                const force =
                    (interactionRadius - distance) / interactionRadius;

                targetX = (dx / distance) * force * 35;
                targetY = (dy / distance) * force * 35;
            }

            // SMOOTH MOVEMENT
            polygon.offsetX += (targetX - polygon.offsetX) * 0.04;
            polygon.offsetY += (targetY - polygon.offsetY) * 0.04;

            drawPolygon(polygon);
        });

        animationFrame = requestAnimationFrame(animate);
    }

    // TRACK CURSOR
    document.addEventListener("pointermove", function (event) {

        cursor.x = event.clientX;
        cursor.y = event.clientY;

    });

    document.addEventListener("pointerleave", function () {

        cursor.x = -1000;
        cursor.y = -1000;

    });

    // INITIALIZE
    resizeCanvas();
    createPolygons();
    animate();

    window.addEventListener("resize", function () {
        resizeCanvas();
        createPolygons();
    });

});
