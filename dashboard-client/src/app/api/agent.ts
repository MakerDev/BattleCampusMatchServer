import axios, { AxiosResponse } from 'axios';
import { IAdminUser, IAdminUserForm } from '../models/user';

axios.defaults.baseURL = 'https://localhost:6001/api';

axios.interceptors.request.use((config) => {
    const token = window.localStorage.getItem('jwt');
    if (token) config.headers.Authorization = `Bearer ${token}`;
    return config;
}, error => {
    return Promise.reject(error);
})

axios.interceptors.response.use(undefined, error => {
    if (error.message === 'Network Error' && !error.response) {
        console.log('Network error - make sure API is running!')
        return;
    }

    const {status, data, config} = error.response;
    if (status === 404) {
        console.log('not found');
        //history.push('/notfound')
    }
    if (status === 400 && config.method === 'get' && data.errors.hasOwnProperty('id')) {
        console.log("400 : not found");
        //history.push('/notfound')
    }
    if (status === 500) {
        console.log('Server error - check the terminal for more info!')
    }
    throw error.response;
})


const responseBody = (response: AxiosResponse) => response.data;

const sleep = (ms: number) => (response: AxiosResponse) => 
    new Promise<AxiosResponse>(resolve => setTimeout(() => resolve(response), ms));

const requests = {
    get: (url: string) => axios.get(url).then(sleep(1000)).then(responseBody),
    post: (url: string, body: {}) => axios.post(url, body).then(sleep(1000)).then(responseBody),
    put: (url: string, body: {}) => axios.put(url, body).then(sleep(1000)).then(responseBody),
    del: (url: string) => axios.delete(url).then(sleep(1000)).then(responseBody),
    postForm: (url: string, file: Blob) => {
        let formData = new FormData();
        formData.append('File', file);
        return axios.post(url, formData, {
            headers: {'Content-type': 'multipart/form-data'}
        }).then(responseBody)
    }
};

const User = {
    login: (user: IAdminUserForm): Promise<IAdminUser> => requests.post(`/user/login`, user),
}

export default {
    User
}