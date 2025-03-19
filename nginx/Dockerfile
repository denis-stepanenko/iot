FROM node as build
WORKDIR /app
COPY /frontend .
RUN npm install
RUN npm run build

FROM nginx
COPY --from=build /app/dist /usr/share/nginx/html
#COPY ./nginx.conf /etc/nginx/nginx.conf
#COPY localhost.crt /etc/ssl/certs/nginx.crt
#COPY localhost-privateKey.key /etc/ssl/private/nginx.key

COPY env.sh /docker-entrypoint.d/env.sh
RUN chmod +x /docker-entrypoint.d/env.sh

EXPOSE 80