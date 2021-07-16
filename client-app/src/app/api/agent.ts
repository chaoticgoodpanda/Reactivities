import axios, {AxiosError, AxiosResponse } from 'axios';
import { toast } from 'react-toastify';
import {Activity, ActivityFormValues} from '../models/activity';
import {history} from "../../index";
import {store} from "../stores/Store";
import {User, UserFormValues} from "../models/User";
import {Photo, Profile, UserActivity } from '../models/Profile';
import {PaginatedResult} from "../models/Pagination";

const sleep = (delay: number) => {
    return new Promise((resolve) => {
        setTimeout(resolve, delay)
    })
}

axios.defaults.baseURL = process.env.REACT_APP_API_URL;

//returns token with every single request when we have a token in our commonStore
axios.interceptors.request.use(config => {
        const token = store.commonStore.token;
        if (token) config.headers.Authorization = `Bearer ${token}`
    return config;
    })

axios.interceptors.response.use(async response => 
    {
        //sleep for 1 sec if in development mode to simulate server lag
        if(process.env.NODE_ENV === 'development') await sleep(1000);
        
        //adding pagination to axios interceptor
        const pagination = response.headers['pagination'];
        if (pagination) {
            response.data = new PaginatedResult(response.data, JSON.parse(pagination));
            return response as AxiosResponse<PaginatedResult<any>>;
        }
        return response;
    }, (error: AxiosError) => {
        console.log(error);
        const {data, status, config, headers} = error.response!;
        switch (status) {
            case 400: 
                if(typeof data == 'string') {
                    toast.error(data);
                }
                if(config.method === 'get' && data.errors.hasOwnProperty('id')) {
                    history.push('/not-found');
                }
                if(data.errors) {
                    const modalStateErrors = [];
                    for (const key in data.errors) {
                        if (data.errors[key]) {
                            modalStateErrors.push(data.errors[key])
                        }
                    }
                    throw modalStateErrors.flat();
                } 
                break;
            case 401:
                if (status === 401 && headers['www-authenticate'].startsWith('Bearer error="invalid_token"')) {
                    store.userStore.logout();
                    toast.error('Session expired, please login again');
                }
                break;
            case 404:
                history.push('/not-found');
                break;
            case 500:
                store.commonStore.setServerError(data);
                history.push('/server-error');
                break;
        } 
        return Promise.reject(error);
    
})

const responseBody = <T> (response: AxiosResponse<T>) => response.data;

const requests = {
    get: <T> (url: string) => axios.get<T>(url).then(responseBody),
    post: <T> (url: string, body: {}) => axios.post<T>(url, body).then(responseBody),
    put: <T> (url: string, body: {}) => axios.put<T>(url, body).then(responseBody),
    delete: <T> (url: string) => axios.delete<T>(url).then(responseBody),
}

const Activities = {
    //paginated list result
    list: (params: URLSearchParams) => axios.get<PaginatedResult<Activity[]>>('/activities', {params}).then(responseBody),
    details: (id: string) => requests.get<Activity>(`/activities/${id}`),
    create: (activity: ActivityFormValues) => requests.post<void>('/activities', activity),
    update: (activity: ActivityFormValues) => requests.put<void>(`/activities/${activity.id}`, activity),
    delete: (id: string) => requests.delete<void>(`/activities/${id}`),
    attend: (id: string) => requests.post<void>(`/activities/${id}/attend`, {})
}

const Account = {
    current: () => requests.get<User>('/account'),
    login: (user: UserFormValues) => requests.post<User>('/account/login', user),
    register: (user: UserFormValues) => requests.post<User>('/account/register', user),
    fbLogin: (accessToken: string) => requests.post<User>(`/account/fbLogin?accessToken=${accessToken}`, {}),
    refreshToken: () => requests.post<User>('/account/refreshToken', {})
    
}

const Profiles = {
    get: (username: string) => requests.get<Profile>(`/profiles/${username}`),
    uploadPhoto: (file: Blob) => {
        let formData = new FormData();
        formData.append('File', file);
        return axios.post<Photo>('photos', formData, {
            headers: {'Content-type': 'multipart/form-data'}
        })
    },
    setMainPhoto: (id: string) => requests.post(`/photos/${id}/setMain`, {}),
    deletePhoto: (id: string) => requests.delete(`/photos/${id}`), 
    updateProfile: (profile:Partial<Profile>) => requests.put(`/profiles`, profile),
    updateFollowing: (username: string) => requests.post(`/follow/${username}`, {}),
    listFollowings: (username: string, predicate: string) => 
        requests.get<Profile[]>(`/follow/${username}?predicate=${predicate}`),
    listActivities: (username: string, predicate: string) =>
        requests.get<UserActivity[]>(`/profiles/${username}/activities?predicate=${predicate}`)
}

const Agent = {
    Activities,
    Account,
    Profiles
}

export default Agent;