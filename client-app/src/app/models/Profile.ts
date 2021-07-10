import {User} from "./User";

export interface Profile {
    username: string,
    displayName: string,
    image?: string,
    bio?: string
}

//class can be used as type Profile as well as Profile and won't cause confusion
export class Profile implements Profile {
    constructor(user: User) {
        this.username = user.username;
        this.displayName = user.displayName;
        this.image = user.image;
    }
}