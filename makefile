all: web.decrypt web.encrypt web.ref

web.decrypt: 
	docker build -t "webdecrypt:latest" -f Dockerfile.WebDecrypt .

web.encrypt: 
	docker build -t "webencrypt:latest" -f Dockerfile.WebEncrypt .

web.ref: 
	docker build -t "webref:latest" -f Dockerfile.WebRef .
