all: web.decrypt web.encrypt web.ref

web.decrypt: 
	docker build -t "webref:latest" -f Dockerfile.WebRef .

web.encrypt: 
	docker build -t "webref:latest" -f Dockerfile.WebRef .

web.ref: 
	docker build -t "webref:latest" -f Dockerfile.WebRef .
